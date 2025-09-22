namespace StefaniniCadastroPessoas.Services
{
    /// <summary>
    /// Serviço para validação de CPF
    /// </summary>
    public interface ICpfValidationService
    {
        /// <summary>
        /// Valida se um CPF é válido
        /// </summary>
        /// <param name="cpf">CPF a ser validado (pode conter formatação)</param>
        /// <returns>True se o CPF for válido, False caso contrário</returns>
        bool ValidarCpf(string? cpf);

        /// <summary>
        /// Remove formatação do CPF (pontos e hífen)
        /// </summary>
        /// <param name="cpf">CPF formatado</param>
        /// <returns>CPF apenas com números</returns>
        string RemoverFormatacao(string? cpf);

        /// <summary>
        /// Formata o CPF com pontos e hífen
        /// </summary>
        /// <param name="cpf">CPF apenas com números</param>
        /// <returns>CPF formatado</returns>
        string FormatarCpf(string? cpf);
    }

    /// <summary>
    /// Implementação do serviço de validação de CPF
    /// </summary>
    public class CpfValidationService : ICpfValidationService
    {
        /// <summary>
        /// Valida se um CPF é válido usando o algoritmo oficial
        /// </summary>
        public bool ValidarCpf(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            cpf = RemoverFormatacao(cpf);

            if (cpf.Length != 11 || !cpf.All(char.IsDigit) || cpf.All(c => c == cpf[0]))
                return false;

            // --- ALGORITMO CORRIGIDO ---

            // Cálculo do primeiro dígito verificador
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            string digito = resto.ToString();
            tempCpf += digito;

            // Cálculo do segundo dígito verificador
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            digito += resto.ToString();

            // Verifica se os dígitos calculados são iguais aos do CPF
            return cpf.EndsWith(digito);
        }

        /// <summary>
        /// Remove pontos, hífens e espaços do CPF
        /// </summary>
        public string RemoverFormatacao(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return string.Empty;

            return cpf.Replace(".", "").Replace("-", "").Replace(" ", "");
        }

        /// <summary>
        /// Formata o CPF no padrão XXX.XXX.XXX-XX
        /// </summary>
        public string FormatarCpf(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return string.Empty;

            cpf = RemoverFormatacao(cpf);

            if (cpf.Length != 11)
                return cpf;

            return Convert.ToUInt64(cpf).ToString(@"000\.000\.000\-00");
        }
    }
}

