using System;
using System.Collections.Generic;
using System.Text;

namespace ProjetoFinalCet105.Mobile.Models;

public class Funcionario
{
    public int Id { get; set; }

    public string NomeCompleto { get; set; } = string.Empty;

    public string? Biografia { get; set; }

    public bool Disponivel { get; set; }
}
