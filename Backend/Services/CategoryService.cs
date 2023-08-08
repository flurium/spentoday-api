using Data;
using Data.Models.ProductTables;
using Lib.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static Backend.Services.CategoryService;

namespace Backend.Services;

public class CategoryService
{
    private readonly Db db;

    public CategoryService(Db db)
    {
        this.db = db;
    }

    public record CategoryIds(string Id, string? ParentId);

    public class CategoryPreparement
    {
        public int StartOrder { get; set; }
        public LinkedList<string> CategoriesToAdd { get; set; }
        public IEnumerable<ProductCategory> CategoriesToDelete { get; set; }

        public CategoryPreparement(int startOrder,
            LinkedList<string> categoriesToAdd, IEnumerable<ProductCategory> categoriesToDelete
        )
        {
            StartOrder = startOrder;
            CategoriesToAdd = categoriesToAdd;
            CategoriesToDelete = categoriesToDelete;
        }
    }

    public async Task<CategoryIds?> MainCategory(string uid, string categoryId)
    {
        return await db.Categories
            .Where(x => x.Id == categoryId && x.Shop.OwnerId == uid)
            .Select(x => new CategoryIds(x.Id, x.ParentId)).QueryOne();
    }

    public async Task<CategoryIds?> ParentCategory(string parentId)
    {
        return await db.Categories
            .Where(x => x.Id == parentId)
            .Select(x => new CategoryIds(x.Id, x.ParentId)).QueryOne();
    }

    /// <summary>
    /// Get ids of parent categories. Including <paramref name="categoryParentId"/>
    /// </summary>
    /// <param name="categoryParentId">ParentId of category to start from.</param>
    /// <returns>
    /// [Parent of parent, parent of category]
    /// </returns>
    public async Task<LinkedList<string>?> CategoryParentIds(string? categoryParentId)
    {
        var ids = new LinkedList<string>();

        var currentParentId = categoryParentId;
        while (currentParentId != null)
        {
            var nextParent = await ParentCategory(currentParentId);
            if (nextParent == null) return null;

            ids.AddFirst(nextParent.Id);
            currentParentId = nextParent.ParentId;
        }

        return ids;
    }

    /// <summary>
    /// It doesn't save changes in db! Only add/edit/delete.
    /// </summary>
    public async Task<bool> ChangeCategoryParent(string uid, Category category, string newParentId)
    {
        try
        {
            var parentExist = await db.Categories.Have(x => x.Id == newParentId && x.Shop.OwnerId == uid);
            if (!parentExist) return false;

            // [parent of parent of parent, parent of parent of category, parent of category]
            var newParentCategoryIds = await CategoryParentIds(newParentId);
            if (newParentCategoryIds == null) return false;

            var products = await db.Products
                .Where(x => x.Shop.OwnerId == uid && x.ProductCategories.Any(x => x.CategoryId == category.Id))
                .Include(x => x.ProductCategories)
                .Select(x => new
                {
                    x.Id,
                    // [parent of parent, parent, category, subcategory, subsubcategory]
                    ProductCategories = x.ProductCategories.OrderBy(x => x.Order).AsEnumerable()
                })
                .QueryMany();

            foreach (var product in products)
            {
                // categories to delete = before category
                // [parent of parent, parent]
                var categoriesToDelete = product.ProductCategories.TakeWhile(x => x.CategoryId != category.Id).ToList();

                // categories to update = category and it's subcategories
                // [category, subcategory, subsubcategory]
                var categoriesToUpdate = product.ProductCategories.SkipWhile(x => x.CategoryId != category.Id);

                int order = 0;
                // add new parent categories
                // if category exists in categoriesToDelete, then just change order.
                foreach (var newParentCategoryId in newParentCategoryIds)
                {
                    var categoryInCategoriesToDelete = categoriesToDelete
                        .FirstOrDefault(x => x.CategoryId == newParentCategoryId);

                    if (categoryInCategoriesToDelete != null)
                    {
                        categoryInCategoriesToDelete.Order = order;
                        categoriesToDelete.Remove(categoryInCategoriesToDelete);
                    }
                    else
                    {
                        var newProductCategory = new ProductCategory(product.Id, newParentCategoryId, order);
                        await db.ProductCategories.AddAsync(newProductCategory);
                    }
                    order += 1;
                }

                // remove categories to delete if they don't exist in newParentCategories
                foreach (var categoryToDelete in categoriesToDelete)
                {
                    db.ProductCategories.Remove(categoryToDelete);
                }

                // update categories to update
                foreach (var categoryToUpdate in categoriesToUpdate)
                {
                    categoryToUpdate.Order = order;
                    order += 1;
                }
            }

            category.ParentId = newParentId;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
    }
}