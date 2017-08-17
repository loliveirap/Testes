using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Unip.GFA.Servico.Servicos;
using Unip.GFA.Entidades.Entidades;
using Unip.GFA.Servico.Base;
using Unip.GFA.Site.Models;
using Unip.GFA.Dados.Repositorio.Seguranca;

namespace Unip.GFA.Site.Controllers
{
    public class AutenticacaoController : Controller
    {
        private readonly IUsuarioServico _servicoUsuario;

        public AutenticacaoController(IUsuarioServico servicoUsuario)
        {
            _servicoUsuario = servicoUsuario;
        }
        MapperConfiguration config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UsuarioModel, Usuario>();
            cfg.CreateMap<Usuario, UsuarioModel>();
        });

        #region Action = GET()
        public ActionResult LogOn()
        {
            return View();
        }

        public ActionResult CadastrarSenha(string id)
        {
            var dados = new SenhaModel();
            dados.Id = id;
            return View(dados);
        }
        #endregion

        #region Action = POST()

        [HttpPost]
        public ActionResult LogOn(UsuarioModel dados)
        {
            if (ModelState.IsValid)
            {
                IMapper mapper = config.CreateMapper();
                var retorno = mapper.Map<Usuario, UsuarioModel>(_servicoUsuario.AutenticarUsuario(dados));
                //FormsAuthentication.SetAuthCookie(dados.Id, false);
                switch (retorno.Status)
                {
                    case 1:                        
                        return RedirectToAction("Index", "Home");
                    case 2:
                        //TempData.Add("Atenção", "Usuário ou senha inválidos!");
                        ViewBag.mensagem = "<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-alert' aria-hidden='true'></span> <strong>Usuário ou senha</strong> inválidos!</div>";
                        return View(dados);                        
                    case 3:
                        return RedirectToAction("CadastrarSenha", new { id = retorno.Id });                        
                    case 4:
                        //TempData.Add("Atenção", "* Usuário bloqueado. Entre em contato com o administrador.");
                        ViewBag.mensagem = "<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-alert' aria-hidden='true'></span> <strong>Usuário bloqueado.</strong> Entre em contato com o administrador</div>";
                        return View(dados);
                    case 5:
                        //TempData.Add("Atenção", "Usuário não encontrato em nossa base de dados.");
                        ViewBag.mensagem = "<div class='alert alert-warning' role='alert'><span class='glyphicon glyphicon-alert' aria-hidden='true'></span> Usuário <strong>não encontrato</strong> em nossa base de dados.</div>";
                        return View(dados);
                }
            }
            return View(dados);
        }

        [HttpPost]
        public ActionResult CadastrarSenha(SenhaModel dados)
        {
            if(ModelState.IsValid)
            {
                if (_servicoUsuario.CadastrarSenha(dados.Id, dados.ConfirmarSenha))                
                    return RedirectToAction("LogOn", "Autenticacao");                
                else
                {
                    TempData.Add("Atenção", "* Não foi possível alterar a senha, favor tentar novamente!");
                    return View(dados);
                }
            }
            else            
                return View(dados);
        }

        public ActionResult LogOff()
        {
            _servicoUsuario.LogOff();
            return RedirectToAction("LogOn", "Autenticacao");
        }
        #endregion
    }
}
