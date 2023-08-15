using BulkyBook.DataAccess.Repository.Generic;
using BulkyBook.Models;
using Newtonsoft.Json.Bson;

namespace BulkyBook.DataAccess.Repository.Orders
{
	public interface IOrderHeaderRepository : IGenericRepository<OrderHeaderModel>
	{
		void Update(OrderHeaderModel obj);
		void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
		void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId);
	}
}
