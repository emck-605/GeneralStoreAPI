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
    public class CustomerController : ApiController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // POST (create)
        // api/Customer
        [HttpPost]
        public async Task<IHttpActionResult> CreateCustomer([FromBody] Customer customerModel)
        {
            if (customerModel is null)
            {
                return BadRequest("Your request body cannot be empty.");
            }

            // If the model is valid
            if (ModelState.IsValid)
            {
                _context.Customers.Add(customerModel); // Added but not saved
                await _context.SaveChangesAsync(); // Now it is saved

                return Ok("Customer successfully created.");

            }
            // If the model is not valid
            return BadRequest(ModelState);
        }

        // GET ALL (read)
        // api/Customer
        [HttpGet]
        public async Task<IHttpActionResult> GetAllCustomers()
        {
            List<Customer> customers = await _context.Customers.ToListAsync();
            return Ok(customers);
        }

        // GET BY ID
        // api/Customer/{id}
        [HttpGet]
        public async Task<IHttpActionResult> GetCustomersByID([FromUri] int id)
        {
            Customer customer = await _context.Customers.FindAsync(id);

            if (customer != null)
            {
                return Ok(customer);
            }

            return NotFound();
        }

        // PUT (update)
        // api/Customer/{id}
        [HttpPut]
        public async Task<IHttpActionResult> UpdateCustomer([FromUri] int id, [FromBody] Customer updatedCustomer)
        {
            // Check the ids if they match
            if (id != updatedCustomer?.Id)
            {
                return BadRequest("Ids do not match.");
            }

            // Check the ModelState
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Find the customer in the database
            Customer customer = await _context.Customers.FindAsync(id);

            // If the customer does not exist, then do something
            if (customer is null)
            {
                return NotFound();
            }

            // Update the properties
            customer.FirstName = updatedCustomer.FirstName;
            customer.LastName = updatedCustomer.LastName;

            // Save the changes
            await _context.SaveChangesAsync();

            return Ok("The customer was updated.");
        }

        // DELETE
        // api/Restaurant/{id}
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteCustomer([FromUri] int id)
        {
            Customer customer = await _context.Customers.FindAsync(id);

            if (customer is null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);

            if (await _context.SaveChangesAsync() is 1)
            {
                return Ok("The customer was deleted.");
            }

            return InternalServerError();
        }
    }
}
