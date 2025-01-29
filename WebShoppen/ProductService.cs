using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebShoppen.Models;

namespace WebShoppen
{
    internal class ProductService
    {
        public static async Task BrowseProducts( User user)
        {
            using var db = new AppDbContext();
            var products = await db.Products.ToListAsync();
            Console.WriteLine("\nAVAILABLE PRODUCTS:");
            foreach (var p in products)
                Console.WriteLine($"{p.Id}. {p.Name} - {p.Price} SEK");

            Console.Write("\nEnter product ID (0 to cancel): ");
            int productId = Helper.GetValidInteger();
            if (productId > 0)
            {
                Console.Write("Quantity: ");
                int quantity = Helper.GetValidInteger();
                await AddToCart( user, productId, quantity);
                Console.WriteLine("Added to cart!");
            }
        }

        public static async Task AddToCart(User user, int productId, int quantity)
        {
            using var db = new AppDbContext();
            user.Cart ??= new Cart { UserId = user.Id };

            var existingItem = user.Cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                user.Cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await db.SaveChangesAsync();
        }

        public static async Task RemoveFromCart( User user, int productId)
        {
            using var db = new AppDbContext();
            if (user?.Cart == null || !user.Cart.Items.Any())
            {
                Console.WriteLine("Cart is empty");
                return;
            }

            var itemToRemove = user.Cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (itemToRemove != null)
            {
                user.Cart.Items.Remove(itemToRemove);
                await db.SaveChangesAsync();
                Console.WriteLine("Item removed from cart!");
            }
            else
            {
                Console.WriteLine("Item not found in cart.");
            }
        }





        public static void ViewCart(User user)
        {
            if (user?.Cart == null || !user.Cart.Items.Any())
            {
                Console.WriteLine("Cart is empty");
                return;
            }

            Console.WriteLine("\nYOUR CART:");
            foreach (var item in user.Cart.Items)
            {
                Console.WriteLine($"{item.Product.Name} x{item.Quantity}");
            }
        }

        public static async Task Checkout(AppDbContext db, User user)
        {
            if (user?.Cart == null || !user.Cart.Items.Any())
            {
                Console.WriteLine("Cart is empty");
                return;
            }

            var order = new Order
            {
                UserId = user.Id,
                Items = user.Cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList(),
                Total = user.Cart.Items.Sum(i => i.Quantity * i.Product.Price)
            };

            // Remove cart
            db.Carts.Remove(user.Cart);
            db.Orders.Add(order);
            await db.SaveChangesAsync();
            Console.WriteLine("Order placed successfully!");
        }

    }
}
