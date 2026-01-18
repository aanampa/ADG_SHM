using System.ComponentModel.DataAnnotations;

namespace SHM.AppWebHonorarioMedico.Models;

public class LoginView
{
    public string Username { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
    public string ClientCaptcha { get; set; } = string.Empty;
    public string ActivateCaptcha { get; set; } = string.Empty;
    public string ResetCode { get; set; } = string.Empty;

    public string InstanceName { get; set; } = string.Empty;
    public string InstanceDescription { get; set; } = string.Empty;
    public string LoginBackgroundColor { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string WebRootPath { get; set; } = string.Empty;
}
