
using System.Globalization;
using Ink.Runtime;

public class DialogueChoice
{
    public Choice InkChoice { get; private set; }

    public float LoyaltyDelta { get; private set; }

    public float RequiredLoyalty { get; private set; }

    public bool HasLoyaltyRequirement { get; private set; }

    public bool HasLoyaltyDelta { get; private set; }

    public string Text => InkChoice.text;

    public DialogueChoice(Choice choice)
    {
        InkChoice = choice;

        LoyaltyDelta = 0f;
        RequiredLoyalty = 0f;

        HasLoyaltyRequirement = false;
        HasLoyaltyDelta = false;

        ReadTags();
    }

    private void ReadTags()
    {
        if (InkChoice == null || InkChoice.tags == null)
            return;

        foreach (string tag in InkChoice.tags)
        {
            if (string.IsNullOrWhiteSpace(tag))
                continue;

            string[] parts = tag.Split(new char[] { '=' }, 2);

            if (parts.Length != 2)
                continue;

            string tagName = parts[0].Trim();
            string tagValue = parts[1].Trim();

            if (!float.TryParse(
                tagValue,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float value))
            {
                UnityEngine.Debug.LogWarning(
                    "Não foi possível interpretar a tag: " + tag
                );

                continue;
            }

            switch (tagName)
            {
                case "delta":
                    LoyaltyDelta = value;
                    HasLoyaltyDelta = true;

                    UnityEngine.Debug.Log(
                        "Tag delta encontrada: " + LoyaltyDelta
                    );
                    break;

                case "required":
                    RequiredLoyalty = value;
                    HasLoyaltyRequirement = true;

                    UnityEngine.Debug.Log(
                        "Tag required encontrada: " + RequiredLoyalty
                    );
                    break;
            }
        }
    }

    public bool IsUnlocked(float currentLoyalty)
    {
        if (!HasLoyaltyRequirement)
            return true;

        return currentLoyalty >= RequiredLoyalty;
    }
}
