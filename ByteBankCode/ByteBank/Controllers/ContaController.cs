﻿using ByteBank.Models;
using ByteBank.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
				if (_userManager == null)
				{
					var contextOwin = HttpContext.GetOwinContext();
					_userManager = contextOwin.GetUserManager<UserManager<UsuarioAplicacao>>();
				}
				return _userManager;
			}
			set
			{
				_userManager = value;
			}
		}

		private SignInManager<UsuarioAplicacao, string> _signInManager;
		public SignInManager<UsuarioAplicacao, string> SignInManager
		{
			get
			{
				if (_signInManager == null)
				{
					var contextOwin = HttpContext.GetOwinContext();
					_signInManager = contextOwin.GetUserManager<SignInManager<UsuarioAplicacao, string>>();
				}
				return _signInManager;
			}
			set
			{
				_signInManager = value;
			}
		}

		public IAuthenticationManager AuthenticationManager
		{
			get
			{
				var contextoOwin = Request.GetOwinContext();
				return contextoOwin.Authentication;
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
					return View("AguardandoConfirmacao");

				var resultado = await UserManager.CreateAsync(novoUsuario, modelo.Senha);

				if (resultado.Succeeded)
				{
					// Enviar o email de confirmação
					await EnviarEmailDeConfirmacaoAsync(novoUsuario);
					return View("AguardandoConfirmacao");
				}
				else
				{
					AdicionaErros(resultado);
				}
			}

			// Alguma coisa de errado aconteceu!
			return View(modelo);
		}

		private async Task EnviarEmailDeConfirmacaoAsync(UsuarioAplicacao usuario)
		{
			var token = await UserManager.GenerateEmailConfirmationTokenAsync(usuario.Id);

			var linkDeCallback =
				Url.Action(
					"ConfirmacaoEmail",
					"Conta",
					new { usuarioId = usuario.Id, token = token },
					Request.Url.Scheme);

			await UserManager.SendEmailAsync(
				usuario.Id,
				"ByteBank - Confirmação de Email",
				$"Bem vindo ao fórum ByteBank, clique aqui {linkDeCallback} para confirmar seu email!");
		}

		public async Task<ActionResult> ConfirmacaoEmail(string usuarioId, string token)
		{
			if (usuarioId == null || token == null)
				return View("Error");

			var resultado = await UserManager.ConfirmEmailAsync(usuarioId, token);

			if (resultado.Succeeded)
				return RedirectToAction("Index", "Home");
			else
				return View("Error");
		}

		public async Task<ActionResult> Login()
		{
			return View();
		}

		[HttpPost]
		public async Task<ActionResult> Login(ContaLoginViewModel modelo)
		{
			if (ModelState.IsValid)
			{
				var usuario = await UserManager.FindByEmailAsync(modelo.Email);

				if (usuario == null)
					return SenhaOuUsuarioInvalidos();

				var signInResultado =
					await SignInManager.PasswordSignInAsync(
						usuario.UserName,
						modelo.Senha,
						isPersistent: modelo.ContinuarLogado,
						shouldLockout: true);

				switch (signInResultado)
				{
					case SignInStatus.Success:

						if (!usuario.EmailConfirmed)
						{
							AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
							return View("AguardandoConfirmacao");
						}


						return RedirectToAction("Index", "Home");
					case SignInStatus.LockedOut:
						var senhaCorreta = 
							await UserManager.CheckPasswordAsync(
							usuario,
							modelo.Senha);

						if (senhaCorreta)
							ModelState.AddModelError("", "A conta está bloqueada");
						else
							return SenhaOuUsuarioInvalidos();

						break;						

					default:
						return SenhaOuUsuarioInvalidos();
				}
			}

			// Algo de errado aconteceu
			return View(modelo);
		}

		public ActionResult EsqueciSenha()
		{
			return View();
		}

		[HttpPost]
		public async Task<ActionResult> EsqueciSenha(ContaEsqueciSenhaViewModel modelo)
		{
			if (ModelState.IsValid)
			{//Gerar o token de reset da senha, gerar o url e enviar o e-mail
				var usuario = await UserManager.FindByEmailAsync(modelo.Email);

				if (usuario != null)
				{
					var token = UserManager.GeneratePasswordResetTokenAsync(usuario.Id);

					var linkDeCallback =
						Url.Action(
					"ConfirmacaoAlteracaoSenha",
					"Conta",
					new { usuarioId = usuario.Id, token = token },
					Request.Url.Scheme);

					await UserManager.SendEmailAsync(
						usuario.Id,
						"ByteBank - Alteração de senha",
						$"Clique aqui {linkDeCallback} para alterar sua senha");
				}
				return View("EmailAlteracaoEnviado");
			}

			return View();
		}

		public ActionResult ConfirmacaoAlteracaoSenha(string usuarioId, string token)
		{
			var modelo = new ContaConfirmacaoAlteracaoSenhaViewModel
			{
				UsuarioId = usuarioId,
				Token = token
			};
			return View(modelo);
		}

		[HttpPost]
		public async Task<ActionResult> ConfirmacaoAlteracaoSenha(ContaConfirmacaoAlteracaoSenhaViewModel modelo)
		{
			if(ModelState.IsValid)
			{ //verifica o teken, o id do usuario e muda a senha
				var resultadoAteracao = 
					await UserManager.ResetPasswordAsync(//função do .NET que faz o reset da senha
					modelo.UsuarioId, //passagem de parametros
					modelo.Token, 
					modelo.NovaSenha);
				if (resultadoAteracao.Succeeded)
				{
					return RedirectToAction("Index", "Home");
				}
				AdicionaErros(resultadoAteracao);
			}
			return View();
		}

		[HttpPost]
		public ActionResult Logoff()
		{
			AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
			return RedirectToAction("Index", "Home");
		}

		private ActionResult SenhaOuUsuarioInvalidos()
		{
			ModelState.AddModelError("", "Credenciais inválidas!");
			return View("Login");
		}

		private void AdicionaErros(IdentityResult resultado)
		{
			foreach (var erro in resultado.Errors)
				ModelState.AddModelError("", erro);
		}
	}
}