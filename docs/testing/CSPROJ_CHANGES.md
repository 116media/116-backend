# .csproj Changes Required

## 1. `tests/Unit/_116.Unit.Tests.csproj`

Add this line inside the `<ItemGroup>` with other ProjectReferences:

```xml
<ProjectReference Include="..\..\src\Modules\Content\Content\Content.csproj" />
```

## 2. `tests/Fixtures/_116.Tests.Fixtures.csproj`

Add this line inside the `<ItemGroup>` with other ProjectReferences:

```xml
<ProjectReference Include="..\..\src\Modules\Content\Content\Content.csproj" />
```

## 3. `tests/Fixtures/Constants/TestConstants.cs`

Add this new nested class inside `TestConstants`:

```csharp
/// <summary>
/// Constants for Content module entity testing.
/// </summary>
public static class Content
{
    public static class ContentType
    {
        public const int NameMaxLength = 30;
        public const string ValidName = "Article";
        public const string AnotherValidName = "Video";
    }

    public static class PricingTier
    {
        public const int NameMaxLength = 40;
        public const int DescriptionMaxLength = 200;
        public const string ValidName = "base_upload";
        public const string ValidDescription = "Base upload fee for content submission.";
        public const string AnotherValidName = "social_boost";
    }

    public static class PromotionLevel
    {
        public const int NameMaxLength = 40;
        public const string ValidName = "Featured — 7 days";
        public const string AnotherValidName = "À la Une — 14 days";
        public const int ValidDurationDays = 7;
        public const decimal ValidPriceUsd = 49.99m;
        public const decimal ZeroPriceUsd = 0m;
        public const decimal NegativePriceUsd = -1m;
    }

    public static class Tag
    {
        public const int NameMaxLength = 50;
        public const int SlugMaxLength = 60;
        public const string ValidName = "Fally Ipupa";
        public const string ValidSlug = "fally-ipupa";
        public const string AnotherValidName = "Kinshasa";
        public const string AnotherValidSlug = "kinshasa";
        public const string InvalidSlug = "Fally Ipupa"; // uppercase with spaces — invalid
    }

    public static class Category
    {
        public const int NameMaxLength = 60;
        public const int SlugMaxLength = 80;
        public const int DescriptionMaxLength = 300;
        public const string ValidName = "Artist Profile";
        public const string ValidSlug = "artist-profile";
        public const string ValidDescription = "Premium artist profile content category.";
        public const string AnotherValidName = "116 Le Focus";
        public const string AnotherValidSlug = "116-le-focus";
        public const string InvalidSlug = "Artist Profile"; // spaces — invalid
    }

    public static class Customer
    {
        public const int FullNameMaxLength = 100;
        public const int EmailMaxLength = 200;
        public const int PhoneMaxLength = 30;
        public const int CompanyMaxLength = 100;
        public const int NotesMaxLength = 500;
        public const string ValidFullName = "John Artist";
        public const string ValidEmail = "john.artist@example.com";
        public const string AnotherValidEmail = "another.artist@example.com";
        public const string ValidPhone = "+243812345678";
        public const string ValidCompany = "Big Label Records";
        public const string ValidNotes = "VIP client, preferred payment via mobile money.";
    }

    public static class Package
    {
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 500;
        public const string ValidName = "Artist Starter Pack";
        public const string ValidDescription = "Includes 1 Artist Profile + 1 interview video.";
        public const decimal ValidFlatPriceUsd = 299.99m;
        public const decimal ZeroPriceUsd = 0m;
        public const decimal NegativePriceUsd = -1m;
    }

    public static class PackageSlot
    {
        public const int ValidQuantity = 1;
        public const int AnotherValidQuantity = 3;
        public const int InvalidQuantity = 0;
        public const int NegativeQuantity = -1;
    }

    public static class CategoryPricing
    {
        public const decimal ValidPriceUsd = 25.00m;
        public const decimal ZeroPriceUsd = 0m;
        public const decimal NegativePriceUsd = -0.01m;
    }
}
```
