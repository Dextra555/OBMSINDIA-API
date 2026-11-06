using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Services;
using Xunit;

namespace OBMS.WebAPI.Tests;

/// <summary>
/// Unit tests for the pure TDS engine (TdsTaxMath). No database / EF required.
/// Covers: remaining deduction months, eligible salary months (joining/resignation),
/// slab-wise tax for every FY 2026-27 New Regime slab, Section 87A rebate + marginal
/// relief, cess, YTD remaining-monthly spreading, Old Regime, and the Employee 374
/// scenario.
/// </summary>
public class TdsTaxMathTests
{
    // ── FY 2026-27 New Regime slabs (contiguous, matches the Phase 2 migration seed) ──
    private static List<TDSSlabConfiguration> NewRegimeSlabsFY2627() => new()
    {
        Slab(0, 400000, 0m),
        Slab(400000, 800000, 5m),
        Slab(800000, 1200000, 10m),
        Slab(1200000, 1600000, 15m),
        Slab(1600000, 2000000, 20m),
        Slab(2000000, 2400000, 25m),
        Slab(2400000, null, 30m),
    };

    private static TDSFYConfig NewRegimeFY2627Config() => new()
    {
        StandardDeduction = 75000m,
        RebateThreshold = 1200000m,
        RebateAmount = 60000m,
        EducationCessRate = 4m,
        ApplyMarginalRelief = true
    };

    private static TDSSlabConfiguration Slab(decimal min, decimal? max, decimal rate) => new()
    {
        MinIncome = min, MaxIncome = max, TaxRate = rate,
        Regime = "New", FinancialYear = "2026-2027", IsActive = true
    };

    private static TDSComputationInput Input(decimal monthlyGross, int periodMonth, int fyStart = 2026,
        DateTime? joining = null, DateTime? resign = null, decimal prevInc = 0, decimal prevTds = 0,
        decimal ytd = 0, string regime = "New", int ageGroup = 1) => new()
    {
        GrossMonthlyIncome = monthlyGross,
        PeriodMonth = periodMonth,
        PeriodYear = fyStart + (periodMonth >= 4 ? 0 : 1),
        FinancialYearStart = fyStart,
        JoiningDate = joining,
        ResignDate = resign,
        PreviousEmployerIncome = prevInc,
        PreviousEmployerTDS = prevTds,
        YtdTDS = ytd,
        Regime = regime,
        AgeGroup = ageGroup
    };

    // ── RemainingDeductionMonths ──
    [Theory]
    [InlineData(4, 12)]   // Apr
    [InlineData(5, 11)]   // May
    [InlineData(9, 7)]    // Sep
    [InlineData(11, 5)]   // Nov
    [InlineData(12, 4)]   // Dec
    [InlineData(1, 3)]    // Jan
    [InlineData(2, 2)]    // Feb
    [InlineData(3, 1)]    // Mar
    public void RemainingDeductionMonths_AllMonths(int periodMonth, int expected)
        => Assert.Equal(expected, TdsTaxMath.RemainingDeductionMonths(periodMonth));

    // ── EligibleSalaryMonths ──
    [Fact]
    public void EligibleMonths_NoJoiningDate_FullYear()
        => Assert.Equal(12, TdsTaxMath.EligibleSalaryMonths(null, null, 2026));

    [Fact]
    public void EligibleMonths_JoinedBeforeFY_FullYear()
        => Assert.Equal(12, TdsTaxMath.EligibleSalaryMonths(new DateTime(2020, 1, 1), null, 2026));

    [Fact]
    public void EligibleMonths_JoinedSep_FirstMonth7()
        => Assert.Equal(7, TdsTaxMath.EligibleSalaryMonths(new DateTime(2026, 9, 1), null, 2026));

    [Fact]
    public void EligibleMonths_JoinedApril_FullYear()
        => Assert.Equal(12, TdsTaxMath.EligibleSalaryMonths(new DateTime(2026, 4, 1), null, 2026));

    [Fact]
    public void EligibleMonths_ResignCapsEligible()
        => Assert.Equal(4, TdsTaxMath.EligibleSalaryMonths(new DateTime(2026, 9, 1), new DateTime(2026, 12, 15), 2026));

    [Fact]
    public void EligibleMonths_JoinsAfterFYEnd_Zero()
        => Assert.Equal(0, TdsTaxMath.EligibleSalaryMonths(new DateTime(2028, 1, 1), null, 2026));

    // ── SlabTax: exercise every New Regime FY 2026-27 slab boundary ──
    [Fact]
    public void SlabTax_FY2627_NilBandOnly_ZeroTax()
    {
        // taxable 4,00,000 → all in 0% band
        decimal tax = TdsTaxMath.SlabTax(400000m, NewRegimeSlabsFY2627(), null);
        Assert.Equal(0m, tax);
    }

    [Fact]
    public void SlabTax_FY2627_First5pctBand()
    {
        // taxable 5,00,000 → 4L @0 + 1L @5% = 5,000
        decimal tax = TdsTaxMath.SlabTax(500000m, NewRegimeSlabsFY2627(), null);
        Assert.Equal(5000m, tax);
    }

    [Fact]
    public void SlabTax_FY2627_Top30pctBand()
    {
        // taxable 25,00,000 → 0%4L +5%4L +10%4L +15%4L +20%4L +25%4L +30%(1L)
        // = 0 + 20000 + 40000 + 60000 + 80000 + 100000 + 30000 = 330000
        decimal tax = TdsTaxMath.SlabTax(2500000m, NewRegimeSlabsFY2627(), null);
        Assert.Equal(330000m, tax);
    }

    [Fact]
    public void SlabTax_ContiguousBoundaries_NoGaps()
    {
        // taxable 4,00,001 should put 4,00,000 in the 0% band and exactly 1 in the 5% band
        // (proves contiguous slabs leak no ₹1 into a wrong band).
        var details = new List<TDSSlabDetail>();
        TdsTaxMath.SlabTax(400001m, NewRegimeSlabsFY2627(), details);
        Assert.Equal(400000m, details[0].TaxableAmount);
        Assert.Equal(1m, details[1].TaxableAmount);
    }

    // ── Section 87A rebate + marginal relief ──
    [Fact]
    public void Rebate87A_IncomeBelowThreshold_FullRebateCappedAtAmount()
    {
        // taxable 12,00,000 → slab tax = 0 + 5%(4L)=20000 + 10%(4L)=40000 = 60000 → rebate = min(60000, 60000) = 60000
        decimal slabTax = TdsTaxMath.SlabTax(1200000m, NewRegimeSlabsFY2627(), null);
        decimal rebate = TdsTaxMath.Rebate87A(1200000m, slabTax, 1200000m, 60000m, true);
        Assert.Equal(60000m, rebate);
    }

    [Fact]
    public void Rebate87A_IncomeBelowThreshold_TaxWipedOutForLowIncome()
    {
        // taxable 4,05,000 → slab tax = 5% of 5000 = 250 → rebate min(250, 60000) = 250 → net 0
        decimal slabTax = TdsTaxMath.SlabTax(405000m, NewRegimeSlabsFY2627(), null);
        decimal rebate = TdsTaxMath.Rebate87A(405000m, slabTax, 1200000m, 60000m, true);
        Assert.Equal(slabTax, rebate);
    }

    [Fact]
    public void Rebate87A_MarginalRelief_JustAboveThreshold()
    {
        // taxable 12,10,000 → slab tax = 0 + 5%(4L)=20000 + 10%(4L)=40000 + 15%(10,000)=1500 = 61500;
        // excess over 12L = 10,000. Marginal relief caps pre-cess tax at the excess → rebate = 61500 - 10000 = 51500.
        decimal slabTax = TdsTaxMath.SlabTax(1210000m, NewRegimeSlabsFY2627(), null);
        Assert.Equal(61500m, slabTax);
        decimal rebate = TdsTaxMath.Rebate87A(1210000m, slabTax, 1200000m, 60000m, true);
        Assert.Equal(51500m, rebate);
    }

    [Fact]
    public void Rebate87A_WellAboveThreshold_NoRebate()
    {
        // taxable 25,36,035 → far above threshold, no marginal relief (tax < excess)
        decimal slabTax = TdsTaxMath.SlabTax(2536035m, NewRegimeSlabsFY2627(), null);
        decimal rebate = TdsTaxMath.Rebate87A(2536035m, slabTax, 1200000m, 60000m, true);
        Assert.Equal(0m, rebate);
    }

    // ── Full Compute: low income → rebate wipes tax → 0 TDS ──
    [Fact]
    public void Compute_LowIncome_RebateWipesTax_ZeroMonthlyTDS()
    {
        // monthly 40,000, joined Apr (12 months), New FY26-27
        var r = TdsTaxMath.Compute(Input(40000m, 4), NewRegimeFY2627Config(), NewRegimeSlabsFY2627());
        // projected 480000; std 75000; taxable 405000; slab 250; rebate 250; net 0
        Assert.Equal(0m, r.TaxAmount - r.RebateAmount); // tax after rebate = 0
        Assert.Equal(0m, r.TotalTax);
        Assert.Equal(0m, r.MonthlyTDS);
    }

    // ── Full Compute: Employee 374 scenario ──
    [Fact]
    public void Compute_Employee374_Sep2026_MatchesExpected()
    {
        // Employee 374: monthly gross 3,73,005; joined 01-Sep-2026; FY 2026-27; first month (Sep).
        var r = TdsTaxMath.Compute(
            Input(373005m, 9, 2026, joining: new DateTime(2026, 9, 1)),
            NewRegimeFY2627Config(), NewRegimeSlabsFY2627());

        Assert.Equal(7, r.EligibleSalaryMonths);
        Assert.Equal(2611035m, r.AnnualIncome);                 // 373005 × 7
        Assert.Equal(75000m, r.StandardDeduction);
        Assert.Equal(2536035m, r.TaxableIncome);
        Assert.Equal(340810.50m, r.TaxAmount);                  // slab tax
        Assert.Equal(0m, r.RebateAmount);                      // above 12L, no rebate
        Assert.Equal(13632.42m, r.EducationCess);              // 4% cess
        Assert.Equal(354442.92m, r.TotalTax);
        Assert.Equal(7, r.RemainingDeductionMonths);           // Sep → Mar
        Assert.Equal(354442.92m, r.BalanceTax);                // YTD 0, prev TDS 0
        Assert.Equal(50634.70m, r.MonthlyTDS);                  // 354442.92 / 7
    }

    // ── YTD: later month with prior deductions should keep per-month even ──
    [Fact]
    public void Compute_Employee374_Nov2026_WithYTD_StaysEven()
    {
        // At Nov: YTD = Sep + Oct TDS = 2 × 50634.70 = 101269.40; remaining months = 5
        decimal ytd = 50634.70m * 2;
        var r = TdsTaxMath.Compute(
            Input(373005m, 11, 2026, joining: new DateTime(2026, 9, 1), ytd: ytd),
            NewRegimeFY2627Config(), NewRegimeSlabsFY2627());

        Assert.Equal(5, r.RemainingDeductionMonths);
        Assert.Equal(253173.52m, r.BalanceTax);                 // 354442.92 - 101269.40
        Assert.Equal(50634.70m, r.MonthlyTDS);                  // 253173.52 / 5 ≈ 50634.704 → 50634.70
    }

    // ── Previous employer income & TDS reduce remaining liability ──
    [Fact]
    public void Compute_PreviousEmployerIncomeAndTDS_AppliedCorrectly()
    {
        // Same employee but previous employer paid 3,00,000 with 15,000 TDS already deducted.
        var r = TdsTaxMath.Compute(
            Input(373005m, 9, 2026, joining: new DateTime(2026, 9, 1), prevInc: 300000m, prevTds: 15000m),
            NewRegimeFY2627Config(), NewRegimeSlabsFY2627());

        // projected = 373005*7 + 300000 = 2,911,035; std 75000; taxable 2,836,035
        Assert.Equal(2911035m, r.AnnualIncome);
        Assert.Equal(2836035m, r.TaxableIncome);
        // remaining = totalTax - 15000 prev TDS - 0 YTD
        Assert.Equal(r.TotalTax - 15000m, r.BalanceTax);
    }

    // ── Old Regime smoke test ──
    [Fact]
    public void Compute_OldRegime_AppliesOldStdDeductionAndRebate()
    {
        var slabs = new List<TDSSlabConfiguration>
        {
            Slab(0, 250000, 0m), Slab(250000, 500000, 5m),
            Slab(500000, 1000000, 20m), Slab(1000000, null, 30m),
        };
        foreach (var s in slabs) { s.Regime = "Old"; }
        var cfg = new TDSFYConfig
        {
            StandardDeduction = 50000m, RebateThreshold = 500000m, RebateAmount = 12500m,
            EducationCessRate = 4m, ApplyMarginalRelief = false
        };

        // monthly 40,000 × 12 = 4,80,000; std 50,000; taxable 4,30,000 (≤5L → rebate)
        var r = TdsTaxMath.Compute(Input(40000m, 4, 2026), cfg, slabs);
        // slab tax = 5% of (430000-250000)=180000 → 9000; rebate min(9000,12500)=9000 → 0
        Assert.Equal(9000m, r.TaxAmount);
        Assert.Equal(9000m, r.RebateAmount);
        Assert.Equal(0m, r.TotalTax);
    }

    // ── Regression: historical FY 2024-25 behaviour (no rebate) preserved ──
    [Fact]
    public void Compute_HistoricalFY2024_25_NoRebate_PreservesPriorBehaviour()
    {
        // FY 2024-25 New Regime seeded config: std 75000, rebate threshold 0, amount 0, cess 4
        var slabs = new List<TDSSlabConfiguration>
        {
            Slab(0, 300000, 0m), Slab(300000, 600000, 5m),
            Slab(600000, 900000, 10m), Slab(900000, 1200000, 15m),
            Slab(1200000, 1500000, 20m), Slab(1500000, null, 30m),
        };
        foreach (var s in slabs) { s.FinancialYear = "2024-2025"; }
        var cfg = new TDSFYConfig
        {
            StandardDeduction = 75000m, RebateThreshold = 0m, RebateAmount = 0m,
            EducationCessRate = 4m, ApplyMarginalRelief = true
        };

        // monthly 40,000 × 12 = 4,80,000; taxable 4,05,000; slab 5% of (405000-300000)=105000 → 5250
        // No rebate (threshold 0). Prior engine charged this tax.
        var r = TdsTaxMath.Compute(Input(40000m, 4, 2024), cfg, slabs);
        Assert.Equal(5250m, r.TaxAmount);
        Assert.Equal(0m, r.RebateAmount);
        Assert.Equal(5250m + 5250m * 0.04m, r.TotalTax); // 5250 + 210 = 5460
    }
}
