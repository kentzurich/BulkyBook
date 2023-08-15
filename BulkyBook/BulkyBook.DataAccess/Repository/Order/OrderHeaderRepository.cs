using BulkyBook.DataAccess.Repository.Generic;
using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository.Orders
{
	public class OrderHeaderRepository : GenericRepository<OrderHeaderModel>, IOrderHeaderRepository
	{
		private ApplicationDBContext _db;
		public OrderHeaderRepository(ApplicationDBContext db) : base(db)
		{
			_db = db;
		}

		public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
		{
			var orderFromDb = _db.OrderHeader.FirstOrDefault(x => x.Id == id);
			if(orderFromDb is not null)
			{
				orderFromDb.OrderStatus = orderStatus;
				if(paymentStatus is not null)
					orderFromDb.PaymentStatus = paymentStatus;
			}
		}
		public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
		{
			var orderFromDb = _db.OrderHeader.FirstOrDefault(x => x.Id == id);

			if(sessionId is not null)
				orderFromDb.SessionId = sessionId;

			if(paymentIntentId is not null)
			{
				orderFromDb.PaymentDate = DateTime.Now;
				orderFromDb.PaymentIntentId = paymentIntentId;
			}
		}

		void IOrderHeaderRepository.Update(OrderHeaderModel obj)
		{
			_db.OrderHeader.Update(obj);
		}
	}
}
