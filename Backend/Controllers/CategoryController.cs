using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Data;
using Data.Models;
using Lib;
using Lib.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly Db db;

        public CategoryController(Db context)
        {
            db = context;
        }

        // POST api/category/{categoryName}
        [HttpPost("{categoryName}")]
        public async Task<IActionResult> CreateOrUpdateCategory(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return BadRequest("Category name cannot be empty.");
            }

            string lowercaseCategoryName = categoryName.ToLower();

            var existingCategory = await db.Categories.QueryOne(c => c.Name.ToLower() == lowercaseCategoryName);

            if (existingCategory != null)
            {
                return Ok(existingCategory);
            }
            else
            {
                var newCategory = new Category(categoryName);
                db.Categories.Add(newCategory);
                await db.Save();
                return Ok(newCategory);
            }
        }

        // DELETE api/category/{categoryId}
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(string categoryId)
        {
            var category = await db.Categories
                .Include(c => c.ProductCategories)
                .QueryOne(c => c.Id == categoryId);

            if (category == null)
            {
                return NotFound();
            }

            if (!category.ProductCategories.Any())
            {
                db.Categories.Remove(category);
            }

            await db.Save();
            return Ok();
        }

        // PUT api/category
        [HttpPut]
        public async Task<IActionResult> EditCategory( [FromBody] Category newCategory)
        {
            var category = await db.Categories.Include(c => c.ProductCategories).QueryOne(c => c.Id == newCategory.Id);

            if (category == null)
            {
                return NotFound();
            }

            string lowercaseNewCategoryName = newCategory.Name.ToLower();

            var existingCategory = await db.Categories.QueryOne(c => c.Name.ToLower() == lowercaseNewCategoryName);

            if (existingCategory != null)
            {
                return Ok(existingCategory);
            }
            else
            {
                if (category.ProductCategories.Any())
                {
                    var createCategory = new Category(newCategory.Name);
                    db.Categories.Add(createCategory);
                    await db.Save();

                    return Ok(createCategory);
                }
                else
                {
                    category.Name = newCategory.Name;
                    await db.Save();

                    return Ok(category);
                }
            }
        }
    }
    }
