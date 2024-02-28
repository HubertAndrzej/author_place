namespace AuthorPlace.Models.Validators;

public interface IRemotePropertyValidator
{
    public string Url { get; }
    public IEnumerable<string> AdditionalFields { get; }
    public string ErrorText { get; }
}
