using ByteBank.App_Start.Identity;
using ByteBank.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

[assembly: OwinStartup(typeof(ByteBank.Startup))]

namespace ByteBank
{


	public class Startup
	{

		public void Configuration(IAppBuilder builder)
		{
			builder.CreatePerOwinContext<DbContext>(() =>
				new IdentityDbContext<UsuarioAplicacao>("DefaultConnection")
			);

			builder.CreatePerOwinContext<IUserStore<UsuarioAplicacao>>(
				(opcoes, contextOwin) =>
				{
					var dbContext = contextOwin.Get<DbContext>();
					return new UserStore<UsuarioAplicacao>(dbContext);
				});

			builder.CreatePerOwinContext<UserManager<UsuarioAplicacao>>(
				(opcoes, contextOwin) =>
				{
					var userStore = contextOwin.Get<IUserStore<UsuarioAplicacao>>();
					var userManager = new UserManager<UsuarioAplicacao>(userStore);

					var userValidator = new UserValidator<UsuarioAplicacao>(userManager);
					userValidator.RequireUniqueEmail = true;

					userManager.UserValidator = userValidator;
					userManager.PasswordValidator = new SenhaValidador() {
						TamanhoRequerido = 6,
						ObrigatorioCaracteresEspeciais = true,
						ObrigatorioLowerCase = true,
						ObrigatorioUpperCase = true,
						ObrigatorioDigitos = true
					};

					userManager.EmailService = new EmailServico();

					var dataProtectionPorvider = opcoes.DataProtectionProvider;
					var dataProtectionPorviderCreate = dataProtectionPorvider.Create("ByteBank");

					userManager.UserTokenProvider = new DataProtectorTokenProvider<UsuarioAplicacao >(dataProtectionPorviderCreate);

					return userManager;
				});
		}

	}
}