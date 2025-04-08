using System.ComponentModel.DataAnnotations;

namespace BookResearchApp.Core.Entities.Constants
{
    public enum BookTypeEnum
    {
        [Display(Name = "Kurgu")]
        Fiction = 1,

        [Display(Name = "Kurgu olmayan")]
        NonFiction = 2,

        [Display(Name = "Bilim")]
        Science = 3,

        [Display(Name = "Tarih")]
        History = 4,

        [Display(Name = "Fantezi")]
        Fantasy = 5,

        [Display(Name = "Biyografi")]
        Biography = 6,

        [Display(Name = "Diğer")]
        Other = 7
    }
}
