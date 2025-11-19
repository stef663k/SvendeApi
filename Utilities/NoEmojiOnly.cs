using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SvendeApi.Utilities;

public class NoEmojiOnly : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string content && !string.IsNullOrEmpty(content))
        {
            var trimmedContent = content.Trim();

            if (string.IsNullOrEmpty(trimmedContent))
                return ValidationResult.Success;

            var onlyEmojis = trimmedContent.EnumerateRunes().All(IsEmoji);

            if (onlyEmojis)
            {
                return new ValidationResult(ErrorMessage ?? "Content cannot only contain emojis");
            }
        }
        return ValidationResult.Success;
    }

    private static bool IsEmoji(Rune rune)
    {
        if (Rune.IsWhiteSpace(rune))
            return false;

        var value = rune.Value;

        return value > 0x1000 && (
            (value >= 0x1F600 && value <= 0x1F64F) || // Emoticons
            (value >= 0x1F300 && value <= 0x1F5FF) || // Symbols & Pictographs
            (value >= 0x1F680 && value <= 0x1F6FF) || // Transport & Map Symbols
            (value >= 0x1F700 && value <= 0x1F77F) || // Alchemical Symbols
            (value >= 0x1F780 && value <= 0x1F7FF) || // Geometric Shapes Extended
            (value >= 0x1F800 && value <= 0x1F8FF) || // Supplemental Arrows-C
            (value >= 0x1F900 && value <= 0x1F9FF) || // Supplemental Symbols and Pictographs
            (value >= 0x1FA00 && value <= 0x1FA6F) || // Chess Symbols
            (value >= 0x1FA70 && value <= 0x1FAFF) || // Symbols and Pictographs Extended-A
            (value >= 0x2600 && value <= 0x26FF)   || // Miscellaneous Symbols
            (value >= 0x2700 && value <= 0x27BF)   || // Dingbats
            (value >= 0xFE00 && value <= 0xFE0F)   || // Variation Selectors
            (value >= 0x1F000 && value <= 0x1F02F) || // Mahjong Tiles
            (value >= 0x1F0A0 && value <= 0x1F0FF)    // Playing Cards
        );
    }
}
 
