using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Data;
using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly Db _context;

        public CategoryController(Db context)
        {
            _context = context;
        }

        // POST api/categories
        [HttpPost]
        public IActionResult CreateOrUpdateCategory([FromBody] string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return BadRequest("Category name cannot be empty.");
            }

            string lowercaseCategoryName = categoryName.ToLower();

            var existingCategory = _context.Categories.FirstOrDefault(c => c.Name.ToLower() == lowercaseCategoryName);

            if (existingCategory != null)
            {
                return Ok(existingCategory);
            }
            else
            {
                var newCategory = new Category(categoryName);
                _context.Categories.Add(newCategory);
                _context.SaveChanges();
                return Ok(newCategory);
            }
        }

        // DELETE api/categories/{categoryId}
        [HttpDelete("{categoryId}")]
        public IActionResult DeleteCategory(string categoryId)
        {
            var category = _context.Categories
                .Include(c => c.ProductCategories)
                .FirstOrDefault(c => c.Id == categoryId);

            if (category == null)
            {
                return NotFound();
            }

            if (category.ProductCategories.Any())
            {
                foreach (var productCategory in category.ProductCategories.ToList())
                {
                    _context.ProductCategories.Remove(productCategory);
                }
            }
            else
            {
                _context.Categories.Remove(category);
            }

            _context.SaveChanges();
            return Ok();
        }

        // PUT api/categories/{categoryId}
        [HttpPut("{categoryId}")]
        public IActionResult EditCategory(string categoryId, [FromBody] string newCategoryName)
        {
            var category = _context.Categories.Include(c => c.ProductCategories).FirstOrDefault(c => c.Id == categoryId);

            if (category == null)
            {
                return NotFound();
            }

            string lowercaseNewCategoryName = newCategoryName.ToLower();

            var existingCategory = _context.Categories.FirstOrDefault(c => c.Name.ToLower() == lowercaseNewCategoryName);

            if (existingCategory != null)
            {
                return Ok(existingCategory);
            }
            else
            {
                if (category.ProductCategories.Any())
                {
                    var newCategory = new Category(newCategoryName);
                    _context.Categories.Add(newCategory);
                    _context.SaveChanges();

                    return Ok(newCategory);
                }
                else
                {
                    category.Name = newCategoryName;
                    _context.SaveChanges();

                    return Ok(category);
                }
            }
        }
    }
    }
