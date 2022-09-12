using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class CustomerBasket
    {
        public CustomerBasket()
        {
        }

        public CustomerBasket(string id)
        {
            Id = id;
        }

        public string Id { get; set; }

        public List<BasketItem> Items { get; set; } = new List<BasketItem>();

        public int? DeliveryMethodId { get; set; }

        // this is going to be used by Stripe so the user can confirm the payment intent and the other property
        public string ClientSecret { get; set; }

        // we'll use this one to be able to update the payment intent if the client makes a change to the delivery
        // method, to the order in any way, they go back to the basket, they add more stuff in after they've
        // already created a payment intent, then we're going to want to update that payment intent rather than creating a new one.
        public string PaymentIntentId { get; set; }

        public decimal ShippingPrice { get; set; }
    }
}