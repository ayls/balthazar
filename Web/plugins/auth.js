export default function ({ $auth }) {
  if (window.config) {
    $auth.strategies.oAuth2.options.authorization_endpoint = window.config.authorization_endpoint;
    $auth.strategies.oAuth2.options.client_id = window.config.authorization_client_id;
  }
}