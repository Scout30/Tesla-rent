using Microsoft.Data.Sqlite;
using System;

class Program
{
    static void Main(string[] args)
    {
        string connectionString = "Data Source=tesla_rental.db";
        var rentalCtrl = new RentalController(connectionString);
        while (true)
        {
            Console.WriteLine("Choose action: 1:'add-car', 2:'list-car' 3:'add-client', 4:'list-client' 5:'rent', 6:'calculate', 7:'list-rent', 8:'stop'");
            string userCommand = Console.ReadLine()?.ToLower();
          
            switch (userCommand)
            {
                case "1":
				case "add-car":
                    rentalCtrl.AddCar();
                    break;
				case "2":
				case "list-car":
                    rentalCtrl.ListCars();
                    break;				
				case "3":	
                case "add-client":
                    rentalCtrl.AddClient();
                    break;
				case "4":
				case "list-clients":
                    rentalCtrl.ListClients();
                    break;
				case "5":
                case "rent":
                    rentalCtrl.RentCar();
                    break;
				case "6":
                case "calculate":
                    rentalCtrl.CalculatePayment();
                    break;
				case "7":
				case "list-rent":
					rentalCtrl.RentList();
                    break;
				case "8":
                case "stop":
                    Console.WriteLine("Good by.");
                    return;
					
                default:
                    Console.WriteLine("Invalid action. Please try again.");
                    break;
            }
        }
    }
}
public class RentalController
{
    private readonly string _connectionString;
    public RentalController(string connectionString)
    {
        _connectionString = connectionString;
        CreateTables();
    }
    private void CreateTables()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        string tableCreationQuery = @"CREATE TABLE IF NOT EXISTS Cars (
			Id INTEGER PRIMARY KEY AUTOINCREMENT, 
			Model nvarchar(100) NOT NULL, 
			HourlyRate decimal(18,2) NOT NULL, 
			KmRate decimal(18,2) NOT NULL);
            CREATE TABLE IF NOT EXISTS Clients (
				Id INTEGER PRIMARY KEY AUTOINCREMENT,
				Name nvarchar(100) NOT NULL, 
				Email nvarchar(100) NOT NULL);
            CREATE TABLE IF NOT EXISTS Rentals (
				Id INTEGER PRIMARY KEY AUTOINCREMENT, 
					ClientId INT NOT NULL, 
					CarId INT NOT NULL, 
					StartTime datetime NOT NULL, 
					EndTime datetime, 
					KmDriven decimal(18,2), 
					Payment decimal(18,2), 
					FOREIGN KEY(ClientId) REFERENCES Clients(Id), 
					FOREIGN KEY(CarId) REFERENCES Cars(Id));";
        using var command = connection.CreateCommand();
        command.CommandText = tableCreationQuery;
        command.ExecuteNonQuery();
    }
    public void AddCar()
    {
        Console.Write("Enter car model: ");
        string model = Console.ReadLine();
        Console.Write("Enter hourly rate (EUR/h): ");
        if (!Decimal.TryParse(Console.ReadLine(), out Decimal hourlyRate))
        {
            Console.WriteLine("Invalid hourly rate.");
            return;
        }
        Console.Write("Enter kilometer rate (EUR/km): ");
        if (!double.TryParse(Console.ReadLine(), out double kmRate))
        {
            Console.WriteLine("Invalid kilometer rate.");
            return;
        }
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Cars (Model, HourlyRate, KmRate) VALUES (@model, @hourlyRate, @kmRate); select last_insert_rowid() AS ID";
        command.Parameters.AddWithValue("@model", model);
        command.Parameters.AddWithValue("@hourlyRate", hourlyRate);
        command.Parameters.AddWithValue("@kmRate", kmRate);
        var id=command.ExecuteScalar();
        Console.WriteLine(string.Format("Car '{1}' added successfully with Id={0}.",id,model));
    }
	
	public void ListCars()
    {
       
        using (var connection = new SqliteConnection(_connectionString)){
			connection.Open();
			using var command = connection.CreateCommand();
			command.CommandText = "select Id, Model, HourlyRate, KmRate from Cars order by Id ";        


			 Console.WriteLine("Cars in DB: ");
			var n=0;
			using( var lasitajs=command.ExecuteReader()){
				while (lasitajs.Read()){
					n++;
					 Console.WriteLine(string.Format("Id ={0}, Model='{1}', HourlyRate={2}, KmRate={3}",
													 Convert.ToInt32(lasitajs["Id"]), 
													 Convert.ToString(lasitajs["Model"]),
													 Convert.ToDouble(lasitajs["HourlyRate"]),
													 Convert.ToDouble(lasitajs["KmRate"])));
				}
			}
			Console.WriteLine(string.Format("Total {0} cars",n));
		}        
    }
    public void AddClient()
    {
        Console.Write("Enter client name: ");
        string name = Console.ReadLine();
        Console.Write("Enter client email: ");
        string email = Console.ReadLine();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Clients (Name, Email) VALUES (@name, @email); select last_insert_rowid() AS ID";
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@email", email);
        var id=command.ExecuteScalar();
        Console.WriteLine(string.Format("Client '{1}' added successfully with Id={0}.",id,name));
       
    }
	
	public void ListClients()
    {
       
        using (var connection = new SqliteConnection(_connectionString)){
			connection.Open();
			using var command = connection.CreateCommand();
			command.CommandText = "select Id, Name, Email from Clients order by Id ";        


			 Console.WriteLine("Clients in DB: ");
			var n=0;
			using( var lasitajs=command.ExecuteReader()){
				while (lasitajs.Read()){
					n++;
					 Console.WriteLine(string.Format("Id ={0}, Name='{1}', Email='{2}'",
													 Convert.ToInt32(lasitajs["Id"]), 
													 lasitajs["Name"],
													 lasitajs["Email"]
													 ));
				}
			}
			Console.WriteLine(string.Format("Total {0} clients",n));
		}        
    }
	
	
	public void RentList()
    {
       
        using (var connection = new SqliteConnection(_connectionString)){
			connection.Open();
			using var command = connection.CreateCommand();
			command.CommandText = @"select R.Id, 
					Name, 
					Model, 
					StartTime, 
					EndTime, 
					KmDriven, 
					Payment
			from Rentals AS R 
			inner join Cars as c on c.Id=CarId
			inner join Clients as b on b.Id=ClientId
			order by R.Id ";        


			 Console.WriteLine("Rentals in DB: ");
			var n=0;
			using( var lasitajs=command.ExecuteReader()){
				while (lasitajs.Read()){
					n++;
					Console.WriteLine(string.Format("Id ={0}, Name='{1}', Model='{2}' from {3} to {4} driven {5} km for {6:N2} EUR.",
													 Convert.ToInt32(lasitajs["Id"]), 
													 lasitajs["Name"],
													 lasitajs["Model"],
													 DateTime.Parse(lasitajs["StartTime"].ToString()),
													 (DateTime.TryParse(lasitajs["EndTime"].ToString(), out DateTime endT)?endT:""),
													 (Decimal.TryParse(lasitajs["KmDriven"].ToString(), out Decimal km)?km:null),
													 (Decimal.TryParse(lasitajs["Payment"].ToString(), out Decimal eur)?eur:null)												
													 ));
				}
			}
			Console.WriteLine(string.Format("Total {0} records",n));
		}        
    }
	
    public void RentCar()
    {
        Console.Write("Enter client ID: ");
        if (!int.TryParse(Console.ReadLine(), out int clientId))
        {
            Console.WriteLine("Invalid client ID.");
            return;
        }
        Console.Write("Enter car ID: ");
        if (!int.TryParse(Console.ReadLine(), out int carId))
        {
            Console.WriteLine("Invalid car ID.");
            return;
        }
        Console.Write("Enter start time (YYYY-MM-DD HH:MM): ");
        var startTimeText = Console.ReadLine();
		DateTime startTime;
		if(!DateTime.TryParse(startTimeText, out startTime)){
			Console.WriteLine("Invalid date format.");
            return;
		}
		
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO Rentals (ClientId, CarId, StartTime) 
				select a.Id, c.Id, @startTime
					from Clients as a
					inner join Cars c on c.Id=@carId and a.Id=@clientId
				; select last_insert_rowid() AS ID
				";
        command.Parameters.AddWithValue("@clientId", clientId);
        command.Parameters.AddWithValue("@carId", carId);
        command.Parameters.AddWithValue("@startTime", startTime);
        var id=command.ExecuteScalar();
        Console.WriteLine(string.Format("Car rented successfully. Id={0}.",id));
      
    }
    public void CalculatePayment()
    {
        Console.Write("Enter rental ID: ");
        if (!int.TryParse(Console.ReadLine(), out int rentalId))
        {
            Console.WriteLine("Invalid rental ID.");
            return;
        }
        Console.Write("Enter end time (YYYY-MM-DD HH:MM): ");
       
		var endTimeText = Console.ReadLine();
		DateTime endTime;
		if(!DateTime.TryParse(endTimeText, out endTime)){
			Console.WriteLine("Invalid date format.");
            return;
		}
		
		
        Console.Write("Enter kilometers driven: ");
        if (!double.TryParse(Console.ReadLine(), out double kmDriven))
        {
            Console.WriteLine("Invalid kilometers driven.");
            return;
        }
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        string selectQuery = @"SELECT R.StartTime, C.HourlyRate, C.KmRate FROM Rentals R JOIN Cars C ON R.CarId = C.Id WHERE R.Id = @rentalId";
        using var selectCommand = connection.CreateCommand();
        selectCommand.CommandText = selectQuery;
        selectCommand.Parameters.AddWithValue("@rentalId", rentalId);
        using var reader = selectCommand.ExecuteReader();
        if (reader.Read())
        {
            DateTime startTime = DateTime.Parse(reader["StartTime"].ToString());
            
            double hourlyRate = Convert.ToDouble(reader["HourlyRate"]);
            double kmRate = Convert.ToDouble(reader["KmRate"]);
			System.TimeSpan laiks=endTime - startTime;
            double hours =laiks.TotalHours;
            double payment = (hours * hourlyRate) + (kmDriven * kmRate);
            string updateQuery = @"UPDATE Rentals SET EndTime = @endTime, KmDriven = @kmDriven, Payment = @payment WHERE Id = @rentalId";
            using var updateCommand = connection.CreateCommand();
            updateCommand.CommandText = updateQuery;
            updateCommand.Parameters.AddWithValue("@endTime", endTime);
            updateCommand.Parameters.AddWithValue("@kmDriven", kmDriven);
            updateCommand.Parameters.AddWithValue("@payment", payment);
            updateCommand.Parameters.AddWithValue("@rentalId", rentalId);
            updateCommand.ExecuteNonQuery();
            Console.WriteLine($"Payment calculated: {payment:F2} EUR");
        }
        else
        {
            Console.WriteLine("Rental not found.");
        }
    }
}
