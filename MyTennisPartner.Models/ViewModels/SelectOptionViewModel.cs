namespace MyTennisPartner.Models.ViewModels
{

    /// <summary>
    /// interface for select options
    /// </summary>
    public interface ISelectOption
    {
        string Label { get; set; }
        int Value { get; set; }
    }

    /// <summary>
    /// class to use for populating client-side Select (drop-down) controls
    /// </summary>
    public class SelectOptionViewModel : ISelectOption
    {
        public string Label { get; set; }
        public int Value { get; set; }
        public bool Disabled { get; set; }
    }
}