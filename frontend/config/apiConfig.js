const getApiBase = () => {
  if (window.location.protocol === "file:") return "http://localhost:5000/api";
  if (["5500", "3000", "5173", "8080"].includes(window.location.port)) {
    return "http://localhost:5000/api";
  }
  return "/api";
};
