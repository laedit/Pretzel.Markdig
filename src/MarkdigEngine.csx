#r Markdig.dll

using Markdig;

[Export(typeof(ILightweightMarkupEngine))]
public sealed class MarkdigEngine : ILightweightMarkupEngine
{
    private readonly MarkdownPipeline _pipeline; 

    public MarkdigEngine()
    {
        _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseEmojiAndSmiley().Build();
    }

    public string Convert(string markdownContent)
    {
        return Markdown.ToHtml(markdownContent, _pipeline);
    }
}