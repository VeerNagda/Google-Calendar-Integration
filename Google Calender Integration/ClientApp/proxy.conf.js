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

    ],
    headers: {
      Connection: 'Keep-Alive',
      /*'Access-Control-Allow-Origin': 'http://localhost:44416', // allow requests from any origin
      'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, OPTIONS', // specify allowed methods
      'Access-Control-Allow-Headers': 'Origin, Content-Type, Accept, Authorization', // specify allowed headers*/
    },
    secure: false,
    target: target
  }
]

module.exports = PROXY_CONFIG;
