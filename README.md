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

## Build API Architechture

#### ProductsController.cs

Add brand and type part of web domain name and return json data of brands or types

```
[HttpGet("brands")]
public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductBrands()
{
    return Ok(await _repo.GetProductBrandsAsync());
}

[HttpGet("types")]
public async Task<ActionResult<IReadOnlyList<ProductType>>> GetProductTypes()
{
    return Ok(await _repo.GetProductTypesAsync());
}
```

#### Program.cs 

Add services for API running

```
var host = CreateHostBuilder(args).Build();
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<StoreContext>();
        await context.Database.MigrateAsync();
        await StoreContextSeed.SeedAsync(context, loggerFactory);
    }
    catch(Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occured during migration");
    }
}

host.Run();
```

#### Startup.cs

Add repositories to startup

```
services.AddScoped<IProductRepository, ProductRepository>();//add repository
```

#### Entities

Create base entities, brands and types class, all entities class inherit base entities, add fields in products class

#### IProductRepository.cs

Create interface to implement

```
public interface IProductRepository
{
    Task<Product> GetProductByIdAsync(int id);
    Task<IReadOnlyList<Product>> GetProductsAsync();
    Task<IReadOnlyList<ProductBrand>> GetProductBrandsAsync();
    Task<IReadOnlyList<ProductType>> GetProductTypesAsync();
}
```

#### ProductConfiguration.cs

```
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Id).IsRequired();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p =>p.Description).IsRequired();
        builder.Property(p =>p.Price).HasColumnType("decimal(18,2)");
        builder.Property(p =>p.PictureUrl).IsRequired();
        builder.HasOne(b => b.ProductBrand).WithMany()
            .HasForeignKey(p => p.ProductBrandId);
        builder.HasOne(b => b.ProductType).WithMany()
            .HasForeignKey(p => p.ProductTypeId);

    }
}
```

#### ProductRepository.cs 

Implement IProductReository to get list of product, brands and types

```
private readonly StoreContext _context;
public ProductRepository(StoreContext context)
{
    _context = context;
}

public async Task<IReadOnlyList<ProductBrand>> GetProductBrandsAsync()
{
    return await _context.ProductBrands.ToListAsync();
}

public async Task<Product> GetProductByIdAsync(int id)
{
    return await _context.Products
        .Include(p => p.ProductType)
        .Include(p => p.ProductBrand)
        .FirstOrDefaultAsync(p => p.Id == id);
}

public async Task<IReadOnlyList<Product>> GetProductsAsync()
{
    return await _context.Products
        .Include(p => p.ProductType)
        .Include(p => p.ProductBrand)
        .ToListAsync();
}

public async Task<IReadOnlyList<ProductType>> GetProductTypesAsync()
{
    return await _context.ProductTypes.ToListAsync();
}
```

#### StoreContext.cs 

Create Dbset of brands and types

```
public DbSet<ProductBrand> ProductBrands { get; set; }
public DbSet<ProductType> ProductTypes { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

}
```

#### StoreContextSeed.cs 

Add data in json file into list to use

```
try
{
    if (!context.ProductBrands.Any())
    {
        var brandsData = 
            File.ReadAllText("../Infrastructure/Data/SeedData/brands.json");

        var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandsData);

        foreach (var item in brands)
        {
            context.ProductBrands.Add(item);
        }

        await context.SaveChangesAsync();
    }

    if (!context.ProductTypes.Any())
    {
        var typesData = 
            File.ReadAllText("../Infrastructure/Data/SeedData/types.json");

        var types = JsonSerializer.Deserialize<List<ProductType>>(typesData);

        foreach (var item in types)
        {
            context.ProductTypes.Add(item);
        }

        await context.SaveChangesAsync();
    }

    if (!context.Products.Any())
    {
        var productsData = 
            File.ReadAllText("../Infrastructure/Data/SeedData/products.json");

        var products = JsonSerializer.Deserialize<List<Product>>(productsData);

        foreach (var item in products)
        {
            context.Products.Add(item);
        }

        await context.SaveChangesAsync();
    }
}
catch (Exception ex)
{
    var logger = loggerFactory.CreateLogger<StoreContextSeed>();
    logger.LogError(ex.Message);
}
```

## Build API Generic Repository

#### API.csproj

Add Automapper package

```
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="8.1.1" />
```

#### ProductToReturnDto.cs 

Create Dto class show to client instead of show details of product class

```
public class ProductToReturnDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string PictureUrl { get; set; }
    public string ProductType { get; set; }
    public string ProductBrand { get; set; }
}
```

#### MappingProfiles.cs 

Add automapper files class to use automapper

```
public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Product, ProductToReturnDto>()
            //instead of getting a string of ProductBrand class, get the Name field of ProductBrand
            //same as ProductType
            .ForMember(d => d.ProductBrand, o => o.MapFrom(s => s.ProductBrand.Name))
            .ForMember(d => d.ProductType, o => o.MapFrom(s => s.ProductType.Name))
            //get picture url from producturlresolver class
            .ForMember(d => d.PictureUrl, o => o.MapFrom<ProductUrlResolver>());
    }
}
```

#### ProductUrlResolver.cs

Add this class to show the real url of image file

```
public class ProductUrlResolver : IValueResolver<Product, ProductToReturnDto, string>
{
    private readonly IConfiguration _config;
    public ProductUrlResolver(IConfiguration config)
    {
        _config = config;
    }

    public string Resolve(Product source, ProductToReturnDto destination, string destMember, ResolutionContext context)
    {
        if (!string.IsNullOrEmpty(source.PictureUrl))
        {
            return _config["ApiUrl"] + source.PictureUrl;
        }

        return null;
    }
}
```

#### Startup.cs

Add generic repository, automapper and can use static files

```
//add generic repository
services.AddScoped(typeof(IGenericRepository<>), (typeof(GenericRepository<>)));
//add automapper
services.AddAutoMapper(typeof(MappingProfiles));

//let system to use static files like local image files
app.UseStaticFiles();
```

#### appsettings.Development.json 

```
"ApiUrl": "https://localhost:5001/"
```

#### IGenericRepository.cs 

Create GenericRepository to implement

```
public interface IGenericRepository<T> where T : BaseEntity
{
    //create two methods, one get by id, another one get all item as a list
    Task<T> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> ListAllAsync();
    Task<T> GetEntityWithSpec(ISpecification<T> spec);
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec);
}
```

#### ISpecification.cs 

Create specification interface

```
public interface ISpecification<T>
{
    //create specification interface
    Expression<Func<T, bool>> Criteria {get; }
    List<Expression<Func<T, object>>> Includes {get; }
}
```

#### BaseSpecification.cs

Create basespecification class to implement interface

```
public class BaseSpecification<T> : ISpecification<T>
{
    public BaseSpecification()
    {
    }

    public BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    public Expression<Func<T, bool>> Criteria {get; }

    public List<Expression<Func<T, object>>> Includes {get; } = 
        new List<Expression<Func<T, object>>>();

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }
}
```

#### GenericRepository.cs 

Create GenericRepository to implement, instead of normal repository

```
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly StoreContext _context;
    public GenericRepository(StoreContext context)
    {
        _context = context;
    }

    public async Task<T> GetByIdAsync(int id)
    {
        //get object by id
        return await _context.Set<T>().FindAsync();
    }

    public async Task<IReadOnlyList<T>> ListAllAsync()
    {
        //get a list of object
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<T> GetEntityWithSpec(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), spec);
    }
}
```

#### ProductsWithTypesAndBrandsSpecification.cs

Create this class to implement products which has specific type and brand

```
public class ProductsWithTypesAndBrandsSpecification : BaseSpecification<Product>
{
    //get entrance to product, we can add include in controller
    public ProductsWithTypesAndBrandsSpecification()
    {
        AddInclude(x => x.ProductType);
        AddInclude(x => x.ProductBrand);
    }

    //implement the method with criteria
    public ProductsWithTypesAndBrandsSpecification(int id)
         : base(x => x.Id == id)
    {
        AddInclude(x => x.ProductType);
        AddInclude(x => x.ProductBrand);
    }
}
```

#### SpecificationEvaluator.cs

```
public class SpecificationEvaluator<TEntity> where TEntity : BaseEntity
{
    //create evaluator of specification
    //return iqueryable of products
    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, 
    ISpecification<TEntity> spec)
    {
        var query = inputQuery;

        if (spec.Criteria != null)
        {
            query = query.Where(spec.Criteria); //p => p.ProductTypeId == id
        }

        // doing same with include before in ProductRepository class
        query = spec.Includes.Aggregate(query, 
            (current, include) => current.Include(include));

        return query;
    }
}
```


To be continued.


