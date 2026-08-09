using Dapper;
using Discount.API.Entities;
using Npgsql;
using System.Data;

namespace Discount.API.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private IDbConnection connection;
        private readonly IConfiguration _configuration;

        public DiscountRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
        }

        public async Task<Coupon> GetDiscount(string productName)
        {
            var sql = @"SELECT 
                            *
                        FROM
                            Coupon
                        WHERE
                            ProductName = @ProductName";

            var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>(sql, new { ProductName = productName });

            if (coupon == null)
                return new Coupon() { ProductName = "No discount", Amount = 0, Description = "No discount description" };

            return coupon;
        }

        public async Task<bool> CreateDiscount(Coupon coupon)
        {
            var sql = @"INSERT INTO Coupon
                            (ProductName,
                             Description,
                             Amount)
                        VALUES
                            (@ProductName,
                             @Description,
                             @Amount)";

            var affected = await connection.ExecuteAsync(sql, coupon);

            return affected == 0 ? false : true;
        }

        public async Task<bool> UpdateDiscount(Coupon coupon)
        {
            var sql = @"UPDATE
                            Coupon
                        SET
                            ProductName = @ProductName,
                            Description = @Description,
                            Amount = @Amount
                        WHERE
                            Id = @Id";

            var affected = await connection.ExecuteAsync(sql, coupon);

            return affected == 0 ? false : true;
        }
        public async Task<bool> DeleteDiscount(string productName)
        {
            var sql = @"DELETE FROM
                            Coupon
                        WHERE
                            ProductName = @ProductName";

            var affected = await connection.ExecuteAsync(sql, new { ProductName = productName });

            return affected == 0 ? false : true;
        }
    }
}
