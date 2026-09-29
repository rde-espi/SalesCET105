using System;
using System.Collections.Generic;
using System.Text;

namespace ProjetoFinalCet105.Mobile.Models;

public class Servico
{
    public int Id { get; set; }

    public int CategoriaId { get; set; }

    public string CategoriaNome { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public decimal Preco { get; set; }

    public int DuracaoMinutos { get; set; }

    public bool Disponivel { get; set; }
}