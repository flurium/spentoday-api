using Data.Models.ProductTables;

namespace Backend.Features.Categories;

public class SortingCategories
{
    public record class LeveledCategory(string Id, string? ParentId, int Level, string Name);

    /// <summary>
    /// ChatGPT wrote it: https://chat.openai.com/share/5bdfe6d1-de37-4744-b107-4c64ba071eeb
    /// </summary>
    public static List<LeveledCategory> SortCategories(List<Category> categories)
    {
        var sortedCategories = new List<LeveledCategory>();
        var processedCategoryIds = new HashSet<string>();
        var stack = new Stack<Tuple<Category, int>>();

        foreach (var category in categories.OrderByDescending(c => c.ParentId == null))
        {
            stack.Push(Tuple.Create(category, 0));

            while (stack.Count > 0)
            {
                var (currentCategory, currentLevel) = stack.Pop();

                // Check if the category is already processed
                if (processedCategoryIds.Contains(currentCategory.Id))
                    continue;

                sortedCategories.Add(new LeveledCategory(currentCategory.Id, currentCategory.ParentId, currentLevel, currentCategory.Name));
                processedCategoryIds.Add(currentCategory.Id);

                var children = categories.Where(c => c.ParentId == currentCategory.Id);
                foreach (var child in children.Reverse())
                {
                    stack.Push(Tuple.Create(child, currentLevel + 1));
                }
            }
        }

        return sortedCategories;
    }
}