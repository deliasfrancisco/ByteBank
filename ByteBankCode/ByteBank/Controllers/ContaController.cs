using ByteBank.Models;
using ByteBank.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ByteBank.Controllers
{
	public class ContaController : Controller
	{
		private UserManager<UsuarioAplicacao> _userManager;
		public UserManager<UsuarioAplicacao> UserManager
		{
			get
			{
				if(_userManager == null)
				{
					var contextOwin = HttpContext.GetOwinContext();
					_userManager = contextOwin.GetUserManager<UserManager<UsuarioAplicacao>>();
				}
				return _userManager;
			}
		}

		public ActionResult Registrar()
		{
			return View();
		}


		[HttpPost]
		public async Task<ActionResult> Registrar(ContaRegistrarViewModel modelo)
		{
			if (ModelState.IsValid)
			{
				var novoUsuario = new UsuarioAplicacao();

				novoUsuario.Email = modelo.Email;
				novoUsuario.UserName = modelo.UserName;
				novoUsuario.NomeCompleto = modelo.NomeCompleto;

				var usuario = await UserManager.FindByEmailAsync(modelo.Email);
				var usuarioJaExiste = usuario != null;

				if (usuarioJaExiste)
				{
					return View("AguardandoConfirmacao");
				}

				var resultado = await UserManager.CreateAsync(novoUsuario, modelo.Senha);

				if (resultado.Succeeded)
				{
					await EnviarEmailDeConfirmacao(novoUsuario);
					return View("AguardandoConfirmacao");
				}
				else
				{
					AdicionaErros(resultado);
				}	
				//estava sendo ignorado o resultado do create async, então foi criado um metodo para receber o resultado do Succeded que vem do UserManager.CreateAsync (true or false)
				// Se == true ele faz o login, senão foi gerado um metodo para adicionar erros onde ele percorre uma lista de erros e encontrando o valor do erro ele retorna a mensagem 
			}
			return View();
		}

		private async Task EnviarEmailDeConfirmacao(UsuarioAplicacao usuario)
		{
			var token = await UserManager.GenerateEmailConfirmationTokenAsync(usuario.Id);

			var linkDeCallback = Url.Action(
				"ConfirmacaoEmail",
				"Conta",
				new { usuarioId = usuario.Id, token = token },
				Request.Url.Scheme);

			await  UserManager.SendEmailAsync(usuario.Id, "ByteBank - Email de confirmação", $"Bem vindo ao fórum ByteBank, clique aqui {linkDeCallback} para confirmar seu email!");
		}

		public async Task<ActionResult> ConfirmacaoEmail(string usuarioId, string token)
		{
			if (usuarioId == null || token == null)
			{
				return View("Error");
			}

			var resultado = await UserManager.ConfirmEmailAsync(usuarioId, token);

			if (resultado.Succeeded)
				return RedirectToAction("Index", "Home");

			else
				return View("Error");

			throw new NotImplementedException();
		}

		private void AdicionaErros(IdentityResult resultado)
		{
			foreach (var erro in resultado.Errors)
			{
				ModelState.AddModelError("", erro);
			}
		}
	}
}