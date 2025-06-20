using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Reflection;

namespace GDS_Test_data.Helpers
{
	public static class DatabaseHelper
	{
		// public static T MapDataReaderToEntity<T>(OdbcDataReader reader) where T : new()
		// {
		// 	T entity = new T();
		// 	for (int i = 0; i < reader.FieldCount; i++)
		// 	{
		// 		string columnName = reader.GetName(i);
		// 		PropertyInfo property = typeof(T).GetProperty(columnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
		// 		if (property != null && reader[columnName] != DBNull.Value)
		// 		{
		// 			object value = reader[columnName];
		// 			if (property.PropertyType == typeof(float?) && value is float)
		// 			{
		// 				value = (float?)value;
		// 			}
		// 			property.SetValue(entity, Convert.ChangeType(value, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType), null);
		// 		}
		// 	}
		// 	return entity;
		// }


public static T MapDataReaderToEntity<T>(OdbcDataReader reader) where T : new()
{
    T entity = new T();
    for (int i = 0; i < reader.FieldCount; i++)
    {
        string columnName = reader.GetName(i);
        // Use stricter matching to avoid ambiguity
        PropertyInfo property = typeof(T).GetProperty(columnName, BindingFlags.Public | BindingFlags.Instance);
        if (property == null)
        {
            // Try case-insensitive match only if no exact match is found
            property = typeof(T).GetProperty(columnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        }

        if (property != null && reader[columnName] != DBNull.Value)
        {
            object value = reader[columnName];
            if (property.PropertyType == typeof(float?) && value is float)
            {
                value = (float?)value;
            }
            property.SetValue(entity, Convert.ChangeType(value, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType), null);
        }
    }
    return entity;
}
		public static List<T> GetEntitiesFromDatabase<T>(string connectionString, string query, OdbcParameter[] parameters) where T : new()
		{
			List<T> entities = new List<T>();
			using (OdbcConnection connection = new OdbcConnection(connectionString))
			{
				OdbcCommand command = new OdbcCommand(query, connection);
				if (parameters != null)
				{
					command.Parameters.AddRange(parameters);
				}

				try
				{
					connection.Open();
					OdbcDataReader reader = command.ExecuteReader();
					while (reader.Read())
					{
						T entity = MapDataReaderToEntity<T>(reader);
						entities.Add(entity);
					}
					reader.Close();
				}
				catch (OdbcException ex)
				{
					Console.WriteLine("ODBC Error: " + ex.Message);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error: " + ex.Message);
				}
			}
			return entities;
		}

		public static T GetEntityFromDatabase<T>(string connectionString, string query, OdbcParameter[] parameters) where T : new()
		{
			using (OdbcConnection connection = new OdbcConnection(connectionString))
			{
				OdbcCommand command = new OdbcCommand(query, connection);
				if (parameters != null)
				{
					command.Parameters.AddRange(parameters);
				}

				try
				{
					connection.Open();
					OdbcDataReader reader = command.ExecuteReader();
					if (reader.Read())
					{
						return MapDataReaderToEntity<T>(reader);
					}
					reader.Close();
				}
				catch (OdbcException ex)
				{
					Console.WriteLine("ODBC Error: " + ex.Message);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error: " + ex.Message);
				}
			}
			return default(T);
		}






	}
}