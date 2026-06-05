using System;
using System.Collections.Generic;

namespace MyProject.Domain.Entities;

public partial class Account
{
    public int AccountId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Email { get; set; }

    public bool? IsActive { get; set; }

    public int? RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Role? Role { get; set; }

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
