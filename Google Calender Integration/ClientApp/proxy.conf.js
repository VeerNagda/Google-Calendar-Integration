const {env} = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:28547';

const PROXY_CONFIG = [
  {
    context: [
      "/api/login",
      "/api/oauth/callback",
      "/api/oauth/refreshtoken",
      "/api/oauth/revoketoken",
      "/api/calendarevent/eventcreate",
      "/api/calendarevent/hasaccesstoken",
      "/api/calendarevent/recurringevent",
      "/api/task/hasaccesstoken",
      "/api/task/createtask",
      "/api/task/checktask",

    ],
    headers: {
      Connection: 'Keep-Alive',
    },
    secure: false,
    target: target
  }
]

module.exports = PROXY_CONFIG;
