# Assertions — Content / Lookup

Content-types, pricing-tiers, promotion-levels, tags: CRUD + activate/deactivate
+ popular/active reads.

## After (create + side-effect)
```csharp
var request = CreatePricingTierRequestBuilder.Valid();
var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.PricingTiers, request);
response.StatusCode.Should().Be(HttpStatusCode.Created);

var body = await response.ReadAsAsync<AdminCreatePricingTierResponse>();
body.PricingTier.Name.Should().Be(request.Name);

await using var db = CreateDbContext<ContentDbContext>();
(await db.PricingTiers.AnyAsync(t => t.Id == body.PricingTier.Id)).Should().BeTrue();
// duplicate name → ShouldBeProblem(Conflict)
// negative price / invalid spot priority → ShouldBeProblem(BadRequest)
```

Activate/deactivate re-query the flag. Lists assert the seeded item; popular-tags
asserts ordering/usage count if applicable.

## TODO checklist
- [ ] AdminActivateContentTypeEndpointV1Tests.cs
- [ ] AdminActivatePricingTierEndpointV1Tests.cs
- [ ] AdminActivatePromotionLevelEndpointV1Tests.cs
- [ ] AdminCreateContentTypeEndpointV1Tests.cs
- [ ] AdminCreatePricingTierEndpointV1Tests.cs
- [ ] AdminCreatePromotionLevelEndpointV1Tests.cs
- [ ] AdminCreateTagEndpointV1Tests.cs
- [ ] AdminDeactivateContentTypeEndpointV1Tests.cs
- [ ] AdminDeactivatePricingTierEndpointV1Tests.cs
- [ ] AdminDeactivatePromotionLevelEndpointV1Tests.cs
- [ ] AdminDeleteTagEndpointV1Tests.cs
- [ ] AdminGetAllContentTypesEndpointV1Tests.cs
- [ ] AdminGetAllPricingTiersEndpointV1Tests.cs
- [ ] AdminGetAllPromotionLevelsEndpointV1Tests.cs
- [ ] AdminGetAllTagsEndpointV1Tests.cs
- [ ] AdminUpdateContentTypeEndpointV1Tests.cs
- [ ] AdminUpdatePricingTierEndpointV1Tests.cs
- [ ] AdminUpdatePromotionLevelEndpointV1Tests.cs
- [ ] AdminUpdateTagEndpointV1Tests.cs
- [ ] PublicGetActivePromotionLevelsEndpointV1Tests.cs
- [ ] PublicGetAllContentTypesEndpointV1Tests.cs
- [ ] PublicGetAllTagsEndpointV1Tests.cs
- [ ] PublicGetPopularTagsEndpointV1Tests.cs

## Acceptance
- Mutations verify DB; lists assert seeded item; conflicts/validation use `ShouldBeProblem`.
