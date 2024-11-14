using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TuberTreats.Models;
using TuberTreats.Models.DTOs;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
    List<TuberDriver> drivers = new List<TuberDriver>
    {
        new TuberDriver { Id = 1, Name = "Sam Spud", TuberDeliveries = new List<TuberOrder>() },
        new TuberDriver { Id = 2, Name = "Molly Mash", TuberDeliveries = new List<TuberOrder>() },
        new TuberDriver { Id = 3, Name = "Pat Potato", TuberDeliveries = new List<TuberOrder>() }
    };

    List<Customer> customers = new List<Customer>
    {
        new Customer { Id = 1, Name = "Alice Fry", Address = "123 Potato St", TuberOrders = new List<TuberOrder>() },
        new Customer { Id = 2, Name = "Bob Hash", Address = "456 Spud Ave", TuberOrders = new List<TuberOrder>() },
        new Customer { Id = 3, Name = "Carol Crisp", Address = "789 Tater Blvd", TuberOrders = new List<TuberOrder>() },
        new Customer { Id = 4, Name = "Dave Tot", Address = "101 Fry Rd", TuberOrders = new List<TuberOrder>() },
        new Customer { Id = 5, Name = "Emma Russet", Address = "202 Mash Ln", TuberOrders = new List<TuberOrder>() }
    };

    List<Topping> toppings = new List<Topping>
    {
        new Topping { Id = 1, Name = "Sour Cream" },
        new Topping { Id = 2, Name = "Chives" },
        new Topping { Id = 3, Name = "Cheese" },
        new Topping { Id = 4, Name = "Bacon Bits" },
        new Topping { Id = 5, Name = "Butter" }
    };


    List<TuberOrder> orders = new List<TuberOrder>
    {
        new TuberOrder
        {
            Id = 1,
            OrderPlacedOnDate = DateTime.Now.AddDays(-2),
            CustomerId = 1,
            TuberDriverId = 1,
            DeliveredOnDate = DateTime.Now,
            Toppings = new List<Topping>
            {
                new Topping { Id = 1, Name = "Sour Cream"},  // Sour Cream
                new Topping { Id = 2, Name = "Chives"}   // Chives
            }
        },
        new TuberOrder
        {
            Id = 2,
            OrderPlacedOnDate = DateTime.Now.AddDays(-1),
            CustomerId = 2,
            TuberDriverId = 2,
            DeliveredOnDate = DateTime.Now,
            Toppings = new List<Topping>
            {
                new Topping { Id = 3, Name = "Cheese"},  // Cheese
                new Topping { Id = 4, Name = "Bacon Bits"}  // Bacon Bits
            }
        },
        new TuberOrder
        {
            Id = 3,
            OrderPlacedOnDate = DateTime.Now,
            CustomerId = 3,
            TuberDriverId = 3,
            DeliveredOnDate = null,  // Not yet delivered
            Toppings = new List<Topping>()
        }
    };

    List<TuberTopping> tuberToppings = new List<TuberTopping>
    {
        new TuberTopping
        {
            Id = 1,
            TuberOrderId = 1,  // Reference to TuberOrder with Id = 1
            ToppingId = 1,     // Reference to Topping with Id = 1 (Sour Cream)
        },
        new TuberTopping
        {
            Id = 2,
            TuberOrderId = 1,  // Reference to TuberOrder with Id = 1
            ToppingId = 2,     // Reference to Topping with Id = 2 (Chives)
        },
        new TuberTopping
        {
            Id = 3,
            TuberOrderId = 2,  // Reference to TuberOrder with Id = 2
            ToppingId = 3,     // Reference to Topping with Id = 3 (Cheese)
        },
        new TuberTopping
        {
            Id = 4,
            TuberOrderId = 2,  // Reference to TuberOrder with Id = 2
            ToppingId = 4,     // Reference to Topping with Id = 4 (Bacon Bits)
        }
    };



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//add endpoints here
app.MapGet("/orders", ()=>{
    return orders.Select(o => {
        List<TuberTopping> tuberToppingsForOder = tuberToppings.Where(tt => tt.TuberOrderId == o.Id).ToList(); 
        List <Topping> orderToppings = tuberToppingsForOder.Select(tt => toppings.First(t => t.Id == tt.ToppingId)).ToList();
        return new TuberOrderDTO
        {
           Id = o.Id,
           OrderPlacedOnDate = o.OrderPlacedOnDate,
           CustomerId = o.CustomerId,
           TuberDriverId = o.TuberDriverId,
           DeliveredOnDate = o.DeliveredOnDate,
           Toppings = orderToppings.Select(ot => {
            return new ToppingDTO {
                Id = ot.Id,
                Name = ot.Name
            };
           }).ToList()
           

           
        };
    }).ToList();

});

app.MapGet("/orders/{id}", (int id)=>{
   TuberOrder foundOrder = orders.FirstOrDefault(o => o.Id == id);
   if (foundOrder == null)
   {
    return Results.NotFound();
   }

   Customer foundCustomerOnOrder = customers.FirstOrDefault(c => c.Id == foundOrder.CustomerId);
   CustomerDTO customerDTO = foundCustomerOnOrder != null ? new CustomerDTO
    {
        Id = foundCustomerOnOrder.Id,
        Name = foundCustomerOnOrder.Name,
        Address = foundCustomerOnOrder.Address
    } : null;
   
   TuberDriver foundDriverOnOrder = drivers.FirstOrDefault(d => d.Id == foundOrder.TuberDriverId);
    TuberDriverDTO driverDTO = foundDriverOnOrder != null ? new TuberDriverDTO
    {
        Id = foundDriverOnOrder.Id,
        Name = foundDriverOnOrder.Name
    } : null;
    
    List<TuberTopping> tuberToppingsForOder = tuberToppings
    .Where(tt => tt.TuberOrderId == foundOrder.Id)
    .ToList(); 
    
    List <Topping> orderToppings = tuberToppingsForOder
    .Select(tt => toppings.FirstOrDefault(t => t.Id == tt.ToppingId))
    .Where(t => t != null)
    .ToList();

   return Results.Ok( new TuberOrderDTO 
   {
    Id = foundOrder.Id,
    OrderPlacedOnDate = foundOrder.OrderPlacedOnDate,
    CustomerId = foundOrder.CustomerId,
    Customer = customerDTO,
    TuberDriverId = foundOrder.TuberDriverId,
    TuberDriver = driverDTO,
    Toppings = orderToppings.Select(ot => new ToppingDTO
        {
            Id = ot.Id,
            Name = ot.Name
        }).ToList(),
    DeliveredOnDate = foundOrder.DeliveredOnDate

   });
});

app.MapPost("/orders", (TuberOrder order)=> {
    Customer foundCustomer = customers.FirstOrDefault(c => c.Id == order.CustomerId);
    if (foundCustomer == null)
    {
        return Results.BadRequest();
    }
    order.Id = orders.Max(o => o.Id) + 1;
    order.OrderPlacedOnDate = DateTime.Now;
    orders.Add(order);
    

    return Results.Created($"/orders/{order.Id}", new TuberOrderDTO
    {
       Id = order.Id,
       OrderPlacedOnDate = order.OrderPlacedOnDate,
       CustomerId = order.CustomerId,
       Customer = new CustomerDTO
       {
        Id = foundCustomer.Id,
        Name = foundCustomer.Name,
        Address = foundCustomer.Address
       },
       TuberDriver = null,
       DeliveredOnDate = null,
       Toppings = new List<ToppingDTO>()
       
    });

});

app.MapPut("orders/{id}", (int id, TuberOrder order)=> {
    TuberOrder orderToBeUpdated = orders.FirstOrDefault(o => o.Id == id);
    if (orderToBeUpdated == null)
    {
        return Results.NotFound();
    }

   
    orderToBeUpdated.TuberDriverId = order.TuberDriverId;

    return Results.NoContent();
});

app.MapPost("orders/{id}/complete", (int id)=> {
     TuberOrder foundOrder = orders.FirstOrDefault(o => o.Id == id);
     if (foundOrder == null)
     {
        return Results.NotFound();
     }

     foundOrder.DeliveredOnDate = DateTime.Now;

     return Results.NoContent();
});

app.MapGet("toppings", ()=>{
    return toppings.Select(t => {
        return new ToppingDTO
        {
            Id = t.Id,
            Name = t.Name
        };
    }).ToList();

});

app.MapGet("toppings/{id}", (int id)=> {
    Topping foundTopping = toppings.FirstOrDefault(t => t.Id == id);

    if (foundTopping == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new ToppingDTO {
        Id = foundTopping.Id,
        Name = foundTopping.Name
    });

});

app.MapGet("tuber-toppings", ()=> {
    return tuberToppings.Select(tt => {
       return new TuberToppingDTO
        {
            Id = tt.Id,
            TuberOrderId = tt.TuberOrderId,
            ToppingId = tt.ToppingId,
        };
    }).ToList();

});

app.MapPost("orders/{id}/add-topping", (int id, Topping topping)=> {
    TuberOrder foundOrder = orders.FirstOrDefault(o => o.Id == id);
    if (foundOrder == null)
    {
        return Results.NotFound();
    }

   

    TuberTopping newTuberTopping = new TuberTopping 
    {
        Id = tuberToppings.Max(tt => tt.Id) + 1,
        TuberOrderId = foundOrder.Id,
        ToppingId = topping.Id,
        TuberOrder = new TuberOrder
        {
            Id = foundOrder.Id,
            OrderPlacedOnDate = foundOrder.OrderPlacedOnDate,
            CustomerId = foundOrder.CustomerId,
            TuberDriverId = foundOrder.TuberDriverId,
            DeliveredOnDate = foundOrder.DeliveredOnDate
        },
        Topping = toppings.FirstOrDefault(t => t.Id == topping.Id)
    };

    tuberToppings.Add(newTuberTopping);

    return Results.Ok(new TuberToppingDTO 
    {
        Id = newTuberTopping.Id,
        TuberOrderId = foundOrder.Id,
        ToppingId = topping.Id,
        TuberOrder = new TuberOrderDTO
        {
            Id = foundOrder.Id,
            OrderPlacedOnDate = foundOrder.OrderPlacedOnDate,
            CustomerId = foundOrder.CustomerId,
            TuberDriverId = foundOrder.TuberDriverId,
            DeliveredOnDate = foundOrder.DeliveredOnDate 
        },
        Topping = new ToppingDTO
        {
            Id = topping.Id,
            Name = topping.Name
        }
    });


});

app.MapDelete("orders/{id}/delete-topping", (int id, [FromBody] Topping topping)=>{
    TuberOrder foundOrder = orders.FirstOrDefault(o => o.Id == id);
    if (foundOrder == null)
    {
        return Results.NotFound();
    }

   TuberTopping foundTuberTopping = tuberToppings.FirstOrDefault(tt => tt.ToppingId == topping.Id && tt.TuberOrderId == id);
   tuberToppings.Remove(foundTuberTopping);

   return Results.NoContent();

});


app.MapGet("customers", ()=> {
    return customers.Select(c => {
        return new CustomerDTO
        {
            Id = c.Id,
            Name = c.Name,
            Address = c.Address
        };
    }).ToList();

});


app.MapGet("customers/{id}", (int id)=> {
    Customer foundCustomer = customers.FirstOrDefault(c => c.Id == id);
    if (foundCustomer == null)
    {
        return Results.NotFound();
    }

    List<TuberOrder> customerTuberOrders = orders
    .Where(o => o.CustomerId == foundCustomer.Id)
    .ToList();

  

return Results.Ok(new CustomerDTO
    {
        Id = foundCustomer.Id,
        Name = foundCustomer.Name,
        Address = foundCustomer.Address,
        TuberOrders = customerTuberOrders.Select(o => {
            List<TuberTopping> orderTuberToppings = tuberToppings
            .Where(tt => tt.TuberOrderId == o.Id)
            .ToList();

            List <Topping> orderToppings = orderTuberToppings
            .Select(ot => toppings.FirstOrDefault(t => t.Id == ot.ToppingId ))
            .Where(t => t != null)
            .ToList();

            TuberDriver orderDriver = drivers
            .FirstOrDefault(d => d.Id == o.TuberDriverId);
            return new TuberOrderDTO
            {
                Id = o.Id,
                OrderPlacedOnDate = o.OrderPlacedOnDate,
                CustomerId = o.CustomerId,
                TuberDriverId = o.TuberDriverId,
                TuberDriver = new TuberDriverDTO
                {
                    Id = orderDriver.Id,
                    Name = orderDriver.Name
                },
                DeliveredOnDate = o.DeliveredOnDate,
                Toppings = orderToppings.Select(ot => {
                    return new ToppingDTO
                    {
                        Id = ot.Id,
                        Name = ot.Name
                    };
                }).ToList()
            };
        }).ToList()


    });

});

app.MapPost("customers", (Customer customer)=> {
    customer.Id = customers.Max(c => c.Id) + 1;
    customers.Add(customer);

    return Results.Created($"customers/{customer.Id}", new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address
    });

});

app.MapDelete("customers/{id}", (int id)=> {
    Customer foundCustomer = customers.FirstOrDefault(c => c.Id == id);

    if (foundCustomer == null)
    {
        return Results.NotFound();
    }

    customers.Remove(foundCustomer);

    return Results.NoContent();

});

app.MapGet("drivers", ()=> {
    return drivers.Select(d => {
        return new TuberDriverDTO
        {
            Id = d.Id,
            Name = d.Name
        };
    }).ToList();

});

app.MapGet("drivers/{id}", (int id)=> {
    TuberDriver foundDriver = drivers.FirstOrDefault(d => d.Id == id);

    if (foundDriver == null)
    {
        return Results.NotFound();
    }

    List<TuberOrder> driverOrders = orders
    .Where(o => o.TuberDriverId == foundDriver.Id).ToList();



    return Results.Ok(new TuberDriverDTO 
    {
        Id = foundDriver.Id,
        Name = foundDriver.Name,
        TuberDeliveries = driverOrders
        .Select(o => {
            Customer foundCustomer = customers
            .FirstOrDefault(c => c.Id == o.CustomerId);

            List<TuberTopping> orderTuberToppings = tuberToppings
            .Where(tt => tt.TuberOrderId == o.Id)
            .ToList();

            List<Topping> orderToppings = orderTuberToppings
            .Select(ott => toppings.FirstOrDefault(t => t.Id == ott.ToppingId))
            .ToList();
            
            return new TuberOrderDTO
            {
                Id = o.Id,
                OrderPlacedOnDate = o.OrderPlacedOnDate,
                CustomerId = o.CustomerId,
                Customer = new CustomerDTO
                {
                    Id = foundCustomer.Id,
                    Name = foundCustomer.Name,
                    Address = foundCustomer.Address
                },
                DeliveredOnDate = o.DeliveredOnDate,
                Toppings = orderToppings
                .Select(ot => {
                    return new ToppingDTO
                    {
                        Id = ot.Id,
                        Name = ot.Name
                    };
                }).ToList()


            };
        }).ToList()
        

    });



});



app.Run();
//don't touch or move this!
public partial class Program { }