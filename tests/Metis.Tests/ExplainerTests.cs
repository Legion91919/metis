using Metis.Triage.Explanation;
using Microsoft.Extensions.Logging.Abstractions;

namespace Metis.Tests;

public class ExplainerTests
{
    private static readonly ExplanationFacts Facts = new(
        "Pier 7 Cold Store", 1, 10, 95.8m,
        [new("Loan-to-value", "53.8%", 15.0m), new("Debt service cover", "1.62x", 10.0m), new("Sponsor track record", "10 completed deals", 8.3m)],
        12.5m, "3f9a1c2e");

    [Fact]
    public async Task Template_writes_one_paragraph_naming_rank_score_and_the_three_movers()
    {
        var text = await new TemplateDealExplainer().ExplainAsync(Facts, CancellationToken.None);

        Assert.StartsWith("Pier 7 Cold Store ranks #1 of 10 with a score of 95.8", text);
        Assert.Contains("loan-to-value of 53.8% (+15.0 points)", text);
        Assert.Contains("sponsor track record of 10 completed deals (+8.3 points)", text);
        Assert.Contains("3f9a1c2e", text);
        Assert.DoesNotContain("\n", text);
        Assert.True(ExplanationGuard.IsAcceptable(text, Facts));
    }

    [Theory]
    [InlineData("Great deal.\n\nApprove it.")]
    [InlineData("Pier 7 Cold Store scores well on loan-to-value and debt service cover.")]
    [InlineData("")]
    public async Task Guard_falls_back_to_the_template_when_the_adapter_breaks_the_rules(string adapterOutput)
    {
        var guarded = new GuardedDealExplainer(new FixedExplainer(adapterOutput), TimeProvider.System, NullLogger<GuardedDealExplainer>.Instance);

        var text = await guarded.ExplainAsync(Facts, CancellationToken.None);

        Assert.Equal(await new TemplateDealExplainer().ExplainAsync(Facts, CancellationToken.None), text);
    }

    [Fact]
    public async Task Guard_keeps_adapter_text_that_follows_the_rules()
    {
        const string good = "Pier 7 Cold Store sits first because loan-to-value, debt service cover and sponsor track record are all strong.";
        var guarded = new GuardedDealExplainer(new FixedExplainer(good), TimeProvider.System, NullLogger<GuardedDealExplainer>.Instance);

        Assert.Equal(good, await guarded.ExplainAsync(Facts, CancellationToken.None));
    }

    [Fact]
    public async Task Guard_falls_back_when_the_adapter_fails()
    {
        var guarded = new GuardedDealExplainer(new FailingExplainer(), TimeProvider.System, NullLogger<GuardedDealExplainer>.Instance);

        var text = await guarded.ExplainAsync(Facts, CancellationToken.None);

        Assert.StartsWith("Pier 7 Cold Store ranks #1", text);
    }

    private sealed class FixedExplainer(string text) : IDealExplainer
    {
        public Task<string> ExplainAsync(ExplanationFacts facts, CancellationToken cancellationToken) => Task.FromResult(text);
    }

    private sealed class FailingExplainer : IDealExplainer
    {
        public Task<string> ExplainAsync(ExplanationFacts facts, CancellationToken cancellationToken) =>
            throw new HttpRequestException("model endpoint unavailable");
    }
}
