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
    public class TransactionController : ApiController
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // POST api/Transaction
        [HttpPost]
        public async Task<IHttpActionResult> CreateTransaction([FromBody] Transaction transactionModel)
        {
            // Check if the Transaction Model is null
            if (transactionModel is null)
                return BadRequest("Your request body cannot be null.");

            // Check if ModelState is Invalid
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find the Product by the transaction.SKU and see that it exists
            var productEntity = await _context.Products.FindAsync(transactionModel.SKU);
            if (productEntity is null)
                return BadRequest($"The target product with SKU: {transactionModel.SKU} does not exist.");

            // Find the Customer by the transaction.CustomerId and see that it exists
            var customerEntity = await _context.Customers.FindAsync(transactionModel.CustomerId);
            if (customerEntity is null)
                return BadRequest($"The target customer with ID: {transactionModel.CustomerId} does not exist.");

            // Verify that the product is in stock
            if (productEntity.IsInStock is false)
                return BadRequest($"{productEntity.Name} is out of stock.");

            // Check that there is enough product to complete the transaction
            if (productEntity.NumberInInventory < transactionModel.ItemCount)
                return BadRequest($"Unable to process transaction. The current stock is {productEntity.NumberInInventory}.");

            // Remove the products that were bought
            int newQuantity = productEntity.NumberInInventory - transactionModel.ItemCount;
            productEntity.NumberInInventory = newQuantity;

            // Create the Transaction
            // Add to the Product Entity
            productEntity.Transactions.Add(transactionModel);
            if (await _context.SaveChangesAsync() != 0)
            {
                return Ok($"You successfully added a transaction for {productEntity.Name}.");
            }

            return InternalServerError();
        }

        // GET 
        // api/Transaction
        [HttpGet]
        public async Task<IHttpActionResult> GetAllTransactions()
        {
            List<Transaction> transactions = await _context.Transactions.ToListAsync();
            return Ok(transactions);
        }

        // GET Transactions by Customer ID
        // api/Transaction?customerId={customerId}
        [HttpGet]
        public async Task<IHttpActionResult> GetTransactionsByCustomerID([FromUri] int customerId)
        {
            Customer customer = await _context.Customers.FindAsync(customerId);

            if (customer != null)
            {
                List<Transaction> transactions = await _context.Transactions.Where(p => p.CustomerId == customerId).ToListAsync();

                return Ok(transactions);
            }
            return NotFound();
        }

        // Get Transaction by Transaction ID
        // api/Transaction?transactionId={transactionId}
        [HttpGet]
        public async Task<IHttpActionResult> GetTransactionsByTransactionID([FromUri] int transactionId)
        {
            Transaction transaction = await _context.Transactions.FindAsync(transactionId);

            if (transaction != null)
            {
                List<Transaction> transactions = await _context.Transactions.Where(p => p.Id == transactionId).ToListAsync();

                return Ok(transactions);
            }
            return NotFound();
        }

        // PUT
        [HttpPut]
        public async Task<IHttpActionResult> UpdateTransaction([FromUri] int id, [FromBody] Transaction updatedTransaction)
        {
            Transaction transaction = await _context.Transactions.FindAsync(id);

            if (id != updatedTransaction?.Id)
                return BadRequest("IDs do not match.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (transaction is null)
                return NotFound();

            var productEntity = await _context.Products.FindAsync(updatedTransaction.SKU);

            int currentQuantity = transaction.ItemCount;

            transaction.CustomerId = updatedTransaction.CustomerId;
            transaction.ItemCount = updatedTransaction.ItemCount;
            transaction.DateOfTransaction = updatedTransaction.DateOfTransaction;

            int newQuantity = (currentQuantity + productEntity.NumberInInventory) - updatedTransaction.ItemCount;
            productEntity.NumberInInventory = newQuantity;

            if (newQuantity <= 0)
                return BadRequest($"Unable to process transaction. The stock would be {productEntity.NumberInInventory}.");

            //if (productEntity.IsInStock is false)
            //    return BadRequest($"{productEntity.Name} is out of stock.");

            if (await _context.SaveChangesAsync() != 0)
                return Ok($"You successfully updated the transaction.");

            return InternalServerError();
        }

        // DELETE
        public async Task<IHttpActionResult> DeleteTransaction([FromUri] int id)
        {
            Transaction transaction = await _context.Transactions.FindAsync(id);

            if (transaction is null)
                return NotFound();

            var originalCount = transaction.ItemCount;

            // Update product inventory to reflect updated transaction
            var productEntity = await _context.Products.FindAsync(transaction.SKU);
            int newQuantity = productEntity.NumberInInventory + originalCount;
            productEntity.NumberInInventory = newQuantity;

            _context.Transactions.Remove(transaction);

            if (await _context.SaveChangesAsync() != 0)
                return Ok($"The transaction was deleted. {originalCount} items were added back to inventory.");

            return InternalServerError();
        }
    }
}
