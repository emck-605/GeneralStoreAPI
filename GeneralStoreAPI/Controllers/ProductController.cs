using GeneralStoreAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace GeneralStoreAPI.Controllers
{
    public class ProductController : ApiController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // POST (create)
        [HttpPost]
        public async Task<IHttpActionResult> CreateProduct([FromBody] Product productModel)
        {
            if (productModel is null)
            {
                return BadRequest("Your request body cannot be empty.");
            }

            if (ModelState.IsValid)
            {
                _context.Products.Add(productModel);
                await _context.SaveChangesAsync();

                return Ok("Product successfully created.");
            }

            return BadRequest(ModelState);
        }

        // GET ALL
        [HttpGet]
        public async Task<IHttpActionResult> GetAllProducts()
        {
            List<Product> products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        // GET BY SKU
        // api/Product?SKU={SKU}
        [HttpGet]
        public async Task<IHttpActionResult> GetProductBySKU([FromUri] string SKU)
        {
            Product product = await _context.Products.FindAsync(SKU);

            if (product != null)
            {
                return Ok(product);
            }
            return NotFound();
        }

        // PUT
        [HttpPut]
        public async Task<IHttpActionResult> UpdateProduct([FromUri] string SKU, [FromBody] Product updatedProduct)
        {
            if (SKU != updatedProduct?.SKU)
            {
                return BadRequest("SKUs do not match.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Product product = await _context.Products.FindAsync(SKU);

            if (product is null)
            {
                return NotFound();
            }

            product.Name = updatedProduct.Name;
            product.Cost = updatedProduct.Cost;
            product.NumberInInventory = updatedProduct.NumberInInventory;

            await _context.SaveChangesAsync();

            return Ok("The product was successfully updated.");
        }

        // DELETE
        [HttpDelete]
        public async Task<IHttpActionResult> RemoveProduct([FromUri] string SKU)
        {
            Product product = await _context.Products.FindAsync(SKU);

            if (product is null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);

            if (await _context.SaveChangesAsync() is 1)
            {
                return Ok("The product was successfully deleted.");
            }

            return InternalServerError();
        }
    }
}
