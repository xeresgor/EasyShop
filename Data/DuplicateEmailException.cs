namespace EasyShop.Data;

/// <summary>
/// Lançada pela camada de dados quando o índice UNIQUE do SQLite recusa um e-mail repetido.
/// Mantém o tipo de exceção do SQLite dentro da camada de dados.
/// </summary>
public class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string email)
        : base($"O e-mail \"{email}\" já está cadastrado.") { }
}
