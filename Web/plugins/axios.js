export default function ({ $axios }) {
  const apiBase = window.config?.apiBase || process.env.apiBase;  
  $axios.setBaseURL(apiBase);
}