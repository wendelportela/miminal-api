using MimimalApi.DTOs;
using MiminalApi.Dominio.Entidades;

namespace MiminalApi.Dominio.interfaces;


public interface IAdministradorServico
{
  Administrador? Login(LoginDTO loginDTO); 
}