using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace romo
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // 1. Configurar CORS de forma explícita
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);

            // 2. Rutas de Web API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // 3. Forzar que siempre devuelva JSON (Evita problemas de formato)
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);
        }
    }
}
