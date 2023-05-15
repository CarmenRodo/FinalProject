using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
public class EmployeeController : Controller
{
    private DataContext _dataContext;
    private UserManager<AppUser> _userManager;
    public EmployeeController(DataContext db, UserManager<AppUser> usrMgr)
    {
        _dataContext = db;
        _userManager = usrMgr;
    }
 public IActionResult Register() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async System.Threading.Tasks.Task<IActionResult> Register(EmployeeWithPassword employeeWithPassword)
    {
        if (ModelState.IsValid)
        {
            Employee employee = employeeWithPassword.Employee;
            if (_dataContext.Employees.Any(c => c.Email == employee.Email))
            {
                ModelState.AddModelError("", "Email must be unique");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    AppUser user = new AppUser
                    {
                        // email and username are synced - this is by choice
                        Email = employee.Email,
                        UserName = employee.Email
                    };
                    // Add user to Identity DB
                    IdentityResult result = await _userManager.CreateAsync(user, employeeWithPassword.Password);
                    if (!result.Succeeded)
                    {
                        AddErrorsFromResult(result);
                    }
                    else
                    {
                        // Assign user to customers Role
                        result = await _userManager.AddToRoleAsync(user, "employee");

                        if (!result.Succeeded)
                        {
                            // Delete User from Identity DB
                            await _userManager.DeleteAsync(user);
                            AddErrorsFromResult(result);
                        }
                        else
                        {
                            // Create customer (Northwind)
                            _dataContext.AddEmployee(employee);
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }
        }
        return View();
    }
        private void AddErrorsFromResult(IdentityResult result)
    {
        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
    }
}