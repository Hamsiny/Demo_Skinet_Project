# Demo_Skinet_Project

This is a web project making by .NET and Angular.

## Build basic API

Create three parts of project, API, Core and Infrastructure.


### API part


#### Startup file

This method gets called by the runtime. Use this method to add services to the container.

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    services.AddDbContext<StoreContext>(x => x.UseSqlite(_config.GetConnectionString("DefaultConnection")));
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    });
}
```

#### appsettings.Development.json

Add connection strings.

```
"ConnectionStrings": {
    "DefaultConnection": "Data source=skinet.db"
}
```

#### ProductsController.cs

Create controller class to return the list of product.

```
private readonly StoreContext _context;
public ProductsController(StoreContext context)
{
    _context = context;
}

[HttpGet]
public async Task<ActionResult<List<Product>>> GetProducts()
{
    //make the action doing Asynchronous
    var products = await _context.Products.ToListAsync();
    return Ok(products);
}

[HttpGet("{id}")]
public async Task<ActionResult<Product>> GetProduct(int id)
{
    return await _context.Products.FindAsync(id);
}
```

### Core part

Create product class

### Infrastructure part

Create StoreContext class to return a list of products

#### InitialCreate.cs

Add product field into table

```
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "Products",
        columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false)
                .Annotation("Sqlite:Autoincrement", true),
            Name = table.Column<string>(type: "TEXT", nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Products", x => x.Id);
        });
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropTable(
        name: "Products");
}
```
        


