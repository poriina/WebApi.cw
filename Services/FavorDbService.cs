using Npgsql;
using WebApi.Models;

namespace WebApi.Services
{
    public class FavorDbService
    {
     
        public async Task InsertFavoriteAsync(favorModel newItem)
        {
            string sqlQuery = "INSERT INTO favorites (mal_id, title, type, personal_note, score) " +
                              "VALUES (@mal_id, @title, @type, @personal_note, @score)";

            using (var connect = new NpgsqlConnection(Constants.Connect))
            {
                await connect.OpenAsync();

                using (var command = new NpgsqlCommand(sqlQuery, connect))
                {
                    command.Parameters.AddWithValue("@mal_id", newItem.MalId);
                    command.Parameters.AddWithValue("@title", newItem.Title ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@type", newItem.Type ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@personal_note", newItem.PersonalNote ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@score", newItem.Score ?? (object)DBNull.Value);

                    await command.ExecuteNonQueryAsync();
                }
            }
        } 
        public async Task<List<favorModel>> GetAllFavoritesAsync()
        {
            var list = new List<favorModel>();
            string sqlQuery = "SELECT id, mal_id, title, type, personal_note, score FROM favorites";

            using (var connect = new NpgsqlConnection(Constants.Connect))
            {
                await connect.OpenAsync();
                using (var command = new NpgsqlCommand(sqlQuery, connect))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new favorModel
                            {
                                Id = reader.GetInt32(0),
                                MalId = reader.GetInt32(1),
                                Title = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Type = reader.IsDBNull(3) ? null : reader.GetString(3),
                                PersonalNote = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Score = reader.IsDBNull(5) ? null : reader.GetFloat(5)
                            });
                        }
                    }
                }
            }
            return list;
        }

        public async Task<bool> UpdateFavoriteAsync(int id, favorModel updatedItem)
        {
            string sqlQuery = "UPDATE favorites SET mal_id = @mal_id, title = @title, type = @type, " +
                              "personal_note = @personal_note, score = @score WHERE id = @id";

            using (var connect = new NpgsqlConnection(Constants.Connect))
            {
                await connect.OpenAsync();
                using (var command = new NpgsqlCommand(sqlQuery, connect))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@mal_id", updatedItem.MalId);
                    command.Parameters.AddWithValue("@title", updatedItem.Title ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@type", updatedItem.Type ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@personal_note", updatedItem.PersonalNote ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@score", updatedItem.Score ?? (object)DBNull.Value);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

     
        public async Task<bool> DeleteFavoriteAsync(int id)
        {
            string sqlQuery = "DELETE FROM favorites WHERE id = @id";

            using (var connect = new NpgsqlConnection(Constants.Connect))
            {
                await connect.OpenAsync();
                using (var command = new NpgsqlCommand(sqlQuery, connect))
                {
                    command.Parameters.AddWithValue("@id", id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
    }
}