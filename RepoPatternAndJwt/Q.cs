// 1: add 2 Class Library
// JwtByUsingRepoPattern install Entityframwork.tool
// JwtByUsingRepoPattern.Core
// JwtByUsingRepoPattern.Ef install Entityframwork , .sqlserver, .tool

// -------------------------------------------------------------------------------------

// 2: add Conection String and Jwt Options For AppSetting
//{
//    "ConnectionStrings": {
//        "DefaultConnection": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=JwtByUsingRepoPattern;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False"
//    },
//  "Logging": {
//        "LogLevel": {
//            "Default": "Information",
//      "Microsoft.AspNetCore": "Warning"
//        }
//    },
//  "AllowedHosts": "*",
//  "JWT": {
//    "Key": "Bm//KL17+VZRqJW3/ipWUeTSCEOurii6pzABRkDlIFk=",
//    "Issuer": "IssuerForJwtByUsingRepoPattern",
//    "Audience": "AudienceForJwtByUsingRepoPattern",
//    "DurationInDays": 7
//  }
//}

// Add Referece form the main project to ef library
// -------------------------------------------------------------------------------------

// 3: Create Jwt in JwtByUsingRepoPattern.Core
//public class JwtSetting
//{
//    public string Key { get; set; }
//    public string Issuer { get; set; }
//    public string Audience { get; set; }
//    public double DurationInDays { get; set; }

//}
// -------------------------------------------------------------------------------------


// Add ApplicationUser in JwtByUsingRepoPattern.Core
//public class ApplicationUser : IdentityUser
//{
//    public string FirstName { get; set; }
//    public string LastName { get; set; }
//}

// -------------------------------------------------------------------------------------


// ApplicationDbContext
// Install-Package Microsoft.AspNetCore.Identity.EntityFrameworkCore -Version 7.0.13

//public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
//{
//    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
//    {

//    }
//}
// -------------------------------------------------------------------------------------
// AddConnectionString and JwtConfig

//builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//    .AddEntityFrameworkStores<ApplicationDbContext>();

//builder.Services.AddDbContext<ApplicationDbContext>(option =>
//    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
//         b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

//builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("JWT"));


//builder.Services.AddAuthentication(
//    option => {
//        option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//        option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    })
//    .AddJwtBearer(
//        op =>
//        {
//            op.RequireHttpsMetadata = false;
//            op.SaveToken = false;

//            op.TokenValidationParameters = new TokenValidationParameters
//            {
//                ValidateIssuerSigningKey = true,
//                ValidateIssuer = true,
//                ValidateAudience = true,
//                ValidateLifetime = true,
//                ValidIssuer = builder.Configuration["JWT:Issuer"],
//                ValidAudience = builder.Configuration["JWT:Audience"],
//                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))

//            };
//        }

//    );
// -------------------------------------------------------------------------------------

// add-migration ...
// update-database

// -------------------------------------------------------------------------------------

// add UnitOfWork interFace in JwtByUsingRepoPattern.Core
//public interface IUnitOfWork : IDisposable
//{
//    public int Complete();
//}


// add UnitOfWork in JwtByUsingRepoPattern.Ef

//public class UnitOfWork : IUnitOfWork
//{
//    private readonly ApplicationDbContext _context;
//    public UnitOfWork(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public int Complete()
//    {
//        return _context.SaveChanges();
//    }

//    public void Dispose()
//    {
//        _context.Dispose();
//    }
//}

//builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();


// -------------------------------------------------------------------------------------

// add AuthModel at Model
//public class AuthModel
//{
//    public string Message { get; set; }
//    public string UserName { get; set; }
//    public bool IsAuthenticated { get; set; }
//    public string Email { get; set; }
//    public List<string> Roles { get; set; }
//    public string Token { get; set; }
//    public DateTime ExpireOn { get; set; }
//}

// RegisterModel
//public class RegisterModel
//{
//    public string FirstName { get; set; }
//    public string LastName { get; set; }
//    public string UserName { get; set; }
//    public string Email { get; set; }
//    public string Password { get; set; }
//}

//TokenRequestModel
//public class TokenRequestModel
//{
//    public string Email { get; set; }
//    public string Password { get; set; }
//}

// -------------------------------------------------------------------------------------

// Add ApplicationRoles
//public class ApplicationRoles
//{
//    public const string AdminRole = "Admin";
//    public const string UserRole = "User";

//}

// add-migration AddRoleForApplication

//protected override void Up(MigrationBuilder migrationBuilder)
//{
//    migrationBuilder.InsertData(
//        table: "AspNetRoles",
//        columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
//        values: new object[] { Guid.NewGuid().ToString(), ApplicationRoles.AdminRole, ApplicationRoles.AdminRole.ToUpper(), Guid.NewGuid().ToString() }
//        );
//    migrationBuilder.InsertData(
//        table: "AspNetRoles",
//        columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
//        values: new object[] { Guid.NewGuid().ToString(), ApplicationRoles.UserRole, ApplicationRoles.UserRole.ToUpper(), Guid.NewGuid().ToString() }
//        );
//}

//protected override void Down(MigrationBuilder migrationBuilder)
//{
//    migrationBuilder.Sql("DELETE FROM [AspNetRoles]");
//}

// -------------------------------------------------------------------------------------

// add IAuthService interface
//public interface IAuthServices
//{
//    Task<AuthModel> RegisterAsync(RegisterModel model);
//    Task<AuthModel> GetTokenAsync(TokenRequestModel model);
//}

// add AuthService
//public class AuthServices : IAuthServices
//{
//    // The UserManager<TUser> class is used to manage user information.
//    // The TUser parameter is typically an instance of a class that extends IdentityUser.
//    // This class provides methods for creating, deleting, updating, and retrieving user information
//    // and managing user-related features like changing passwords, assigning roles, and claims.
//    private readonly UserManager<ApplicationUser> _userManager;


//    private readonly JwtSetting _jwt;
//    // The IOptions<T> interface allows accessing a snapshot of the configuration settings as a strongly
//    // typed object. It's typically used for reading configuration options at application program.cs
//    // and provides a way to access the options in a typed manner
//    // builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("JWT")); in program.cs

//    public AuthServices(UserManager<ApplicationUser> userManager, IOptions<JwtSetting> jwt)
//    {
//        _userManager = userManager;
//        _jwt = jwt.Value;
//    }

//    public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
//    {
//        var authModel = new AuthModel();

//        var user = await _userManager.FindByEmailAsync(model.Email);

//        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
//        {
//            authModel.Message = "Email Or Password is Incorrect";
//            return authModel;
//        }

//        var jwtSecurityToken = await CreateJwtToken(user);
//        var roleList = await _userManager.GetRolesAsync(user);

//        authModel.IsAuthenticated = true;
//        authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
//        authModel.Email = model.Email;
//        //authModel.ExpireOn = jwtSecurityToken.ValidTo;
//        authModel.Roles = roleList.ToList();


//        return authModel;
//    }

//    public async Task<AuthModel> RegisterAsync(RegisterModel model)
//    {
//        if (await _userManager.FindByEmailAsync(model.Email) is not null)
//            return new AuthModel { Message = "Email Is Already Exist" };

//        if (await _userManager.FindByNameAsync(model.UserName) is not null)
//            return new AuthModel { Message = "UserName Is Already Exist" };

//        var user = new ApplicationUser
//        {
//            FirstName = model.FirstName,
//            LastName = model.LastName,
//            Email = model.Email,
//            UserName = model.UserName,
//        };

//        //Add User And Make Hash to Password
//        var result = await _userManager.CreateAsync(user, model.Password);

//        if (!result.Succeeded)
//        {
//            var errorList = string.Empty;
//            foreach (var error in result.Errors)
//            {
//                errorList += $"{error.Description}\n";
//            }
//            return new AuthModel { Message = errorList };
//        }

//        //Add To Role
//        await _userManager.AddToRoleAsync(user, ApplicationRoles.UserRole);

//        var jwtSecurityToken = await CreateJwtToken(user);
//        return new AuthModel
//        {
//            UserName = user.UserName,
//            Email = user.Email,
//            //ExpireOn = jwtSecurityToken.ValidTo,
//            IsAuthenticated = true,
//            Roles = new List<string> { ApplicationRoles.UserRole },
//            Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)
//        };

//    }

//    private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
//    {
//        // This line is calling a method to get the claims for the given user
//        var userClaim = await _userManager.GetClaimsAsync(user);
//        var userRole = await _userManager.GetRolesAsync(user);

//        // Claims in the context of authentication are pieces of information about the user
//        var roleClaim = new List<Claim>();

//        foreach (var role in userRole)
//        {
//            roleClaim.Add(new Claim("role", role));
//        }
//        var claims = new[]
//        {
//            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),  
//            // Sub: The subject of the token, usually the username or user identifier
//            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  
//            // Jti: JWT ID (unique identifier for the token)
//            new Claim(JwtRegisteredClaimNames.Email, user.Email),  
//            // Email: The user's email address
//            new Claim("uid", user.Id)  
//            // Custom claim "uid" to store the user's ID
//        }
//        .Union(userClaim)
//        .Union(roleClaim);

//        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
//        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
//        // SigningCredentials is used to define how the token will be signed to ensure its integrity and authenticity

//        var jwtSecurityToken = new JwtSecurityToken(
//            issuer: _jwt.Issuer,
//            audience: _jwt.Audience,
//            claims: claims,
//            expires: DateTime.Now.AddDays(_jwt.DurationInDays),
//            signingCredentials: signingCredentials
//            );


//        return jwtSecurityToken;
//    }
////}


// -------------------------------------------------------------------------------------
// Add Map
//builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("JWT"));


//builder.Services.AddScoped<IAuthServices, AuthServices>();

// app.UseAuthentication();
//app.UseAuthorization();

// -------------------------------------------------------------------------------------

// Add Authorize Button in Swagger

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

//    // Define the security scheme
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Description = "JWT Authorization header using the Bearer scheme.",
//        Type = SecuritySchemeType.Http,
//        Scheme = "bearer",
//        BearerFormat = "JWT"
//    });
//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            Array.Empty<string>()
//        }
//    });
//});


// -------------------------------------------------------------------------------------
// لحد هنا انت كدا عملت تسجيل الدخول وعمل ايميل بس ليس اكثر 
// -------------------------------------------------------------------------------------

// Add IBaseRepository
// public interface IBaseRepository<T> where T : class 

// Add BaseRepository
// public class BaseRepository<T> : IBaseRepository<T> where T : class

// -------------------------------------------------------------------------------------
// Add Model TestClass
// public class TestClass
//{

//}

//public class TestWithoutAnyFunctionMore
//{

//}
// -------------------------------------------------------------------------------------

// Add ITestRepo As Any Model
// Add More Function

// public interface ITestRepository : IBaseRepository<TestClass>
//  {
//     public void SpicalMethod();
//  }

// Add TestRepo
// public class TestRepository : BaseRepository<TestClass>, ITestRepository
// {
//    public TestRepository(ApplicationDbContext context) : base(context)
//        {

//        }
//    public void SpicalMethod()
//        {
//           throw new NotImplementedException();
//        }
// }

// -------------------------------------------------------------------------------------

// Edit IUnitOfWork
//public interface IUnitOfWork : IDisposable
//{
//    IBaseRepository<TestWithoutAnyFunctionMore> testWithoutAnyFunctionMore { get; }
//    ITestRepository testRepository { get; }
//    public int Complete();
//}

// -------------------------------------------------------------------------------------

// Edit UnitOfWork

//public class UnitOfWork : IUnitOfWork
//{
//    private readonly ApplicationDbContext _context;
//    public ITestRepository testRepository { get; private set; }
//    public IBaseRepository<TestWithoutAnyFunctionMore> testWithoutAnyFunctionMore { get; private set; }
//    public UnitOfWork(ApplicationDbContext context)
//    {
//        _context = context;
//        testWithoutAnyFunctionMore = new BaseRepository<TestWithoutAnyFunctionMore>(context);
//        testRepository = new TestRepository(context);
//    }

//    public int Complete()
//    {
//        return _context.SaveChanges();
//    }

//    public void Dispose()
//    {
//        _context.Dispose();
//    }
//}

// -------------------------------------------------------------------------------------

// add-migration
// update-database
// See Controller and Start 
// -------------------------------------------------------------------------------------

//Start RefreshToken :
// A refresh token is a special token that is used to obtain more access tokens


//التوكن(Access Token)
//يكون صالحًا لفترة قصيرة (مثلًا 15 دقيقة) لتقليل المخاطر في حال تم اختراقه.
//عندما تنتهي صلاحية التوكن، يمكن للمستخدم استخدام الريفريش توكن
//(Refresh Token)
//للحصول على توكين جديد دون الحاجة لتسجيل الدخول مرة أخرى
//الريفريش توكن صالح لفترة أطول (مثلاً أسبوع)، ولكنه لا يبقى صالحًا إلى الأبد.
//عند استخدامه لتجديد التوكن، يتم إصدار ريفريش توكن جديد أيضًا، ويتم إلغاء التوكن القديم.
//يتم حفظ الريفريش توكن بشكل آمن في
//HTTP-only cookies،
//مما يقلل من احتمالية اختراقه باستخدام هجمات مثل XSS (Cross-Site Scripting).
//إذا تم محاولة استخدام الريفريش توكن القديم بعد تجديده، سيتم رفض الطلب بشكل فوري.

// إذا انتهت صلاحية الـ Access Token (مثلاً بعد 15 دقيقة) ولكن الريفريش توكن لا يزال صالحًا، يمكن للمستخدم استخدام الريفريش توكن للحصول على Access Token جديد.
//في هذه الحالة،
//الريفريش توكن نفسه يبقى صالحًا ويظل كما هو، ويتم إصدار
//Access Token
//جديد.
//إذا انتهت صلاحية الريفريش توكن:
//إذا انتهت صلاحية الريفريش توكن (بعد أسبوع، مثلًا)،
//فإن المستخدم لن يستطيع تجديد الـ
//Access Token
//باستخدام الريفريش توكن القديم.
//في هذه الحالة،
//يجب على المستخدم تسجيل الدخول مجددًا للحصول على
//Access Token و Refresh Token
//جديدين.

