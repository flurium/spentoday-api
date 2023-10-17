using Data.Models.ProductTables;

namespace Backend.Features.Categories;

public record struct LeveledCategory(string Id, string? ParentId, int Level, string Name);

public record struct LeveledResult(List<LeveledCategory> Categories, int MaxLevel);

public static class LevelingCategories
{
    public static IEnumerable<LeveledCategory> MapLeveled(this IEnumerable<Category> categories, int level)
    {
        return categories.Select(x => new LeveledCategory(x.Id, x.ParentId, level, x.Name));
    }

    private const int LEVEL_START = 2;

    public static LeveledResult Sort(List<Category> categories)
    {
        var sortedCategories = new List<LeveledCategory>(categories.Count);

        // insert categories with no Parent at first
        var topCategories = categories.Where(x => x.ParentId == null).MapLeveled(1);
        sortedCategories.AddRange(topCategories);

        // levelCategories are already inserted categories waiting to insert children
        var levelCategories = topCategories;
        var level = LEVEL_START;
        List<LeveledCategory> nextLevelCategories = new();
        while (levelCategories.Any())
        {
            nextLevelCategories = new();
            foreach (var category in levelCategories)
            {
                var subCategories = categories.Where(x => x.ParentId == category.Id).MapLeveled(level);
                if (subCategories.Any() == false) continue;

                var categoryIndexInSorted = sortedCategories.FindIndex(x => x.Id == category.Id);
                if (categoryIndexInSorted < 0) categoryIndexInSorted = 0;

                sortedCategories.InsertRange(categoryIndexInSorted + 1, subCategories);
                nextLevelCategories.AddRange(subCategories);
            }
            levelCategories = nextLevelCategories;
            level += 1;
        }

        return new LeveledResult(sortedCategories, level - LEVEL_START);
    }
}