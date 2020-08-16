export default function ({ $auth }) {
  if (window.config) {
    $auth.strategies.oAuth2.options.authorization_endpoint = window.config.authorization_endpoint
  }
}