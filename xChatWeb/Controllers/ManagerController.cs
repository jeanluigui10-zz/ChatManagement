﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using xChatEntities;
using xChatUtilities;
using xChatWeb.Services;

namespace xChatWeb.Controllers
{
    /// <summary>
    /// Controlador para la gestión del Manager
    /// </summary>
    public class ManagerController : Controller
    {
        public ActionResult Index(string paramId, string paramAppId, string paramRolId)
        {
            ObjectResultList<AccountManagerConnect> lstAgents;

            lstAgents = ServiceIntegrationChat.Instance.GetAccountManager(paramId, paramAppId, paramRolId, true);

            return View(lstAgents.Elements);
        }

        public ActionResult AgentList(int agentId)
        {
            ObjectResultList<UserConnect> lstUsers;

            lstUsers = ServiceIntegrationChat.Instance.GetUserConnectByAgentId(agentId);

            return PartialView("_ListUsersChat",lstUsers.Elements);
        }

        public ActionResult GetConversation(int chatId)
        {
            ObjectResultList<ConversationResponseEntity> conversation;

            conversation = ServiceIntegrationChat.Instance.GetConversation(chatId);

            return PartialView("_ConversationList", conversation.Elements);
        }
    }
}