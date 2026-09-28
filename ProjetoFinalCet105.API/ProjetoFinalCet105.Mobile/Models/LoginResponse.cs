using System;
using System.Collections.Generic;
using System.Text;

namespace ProjetoFinalCet105.Mobile.Models;

public class LoginResponse
{
    public string? Token { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string NomeCompleto { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool RequiresTwoFactor { get; set; }

    public IList<string> Roles { get; set; } = new List<string>();
}
