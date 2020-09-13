export default function ({ $auth }) {
  if (window.config) {
    $auth.strategies.oAuth2.options.endpoints.authorization = window.config.authorization_endpoint;
    $auth.strategies.oAuth2.options.clientId = window.config.authorization_client_id;
  }
}
