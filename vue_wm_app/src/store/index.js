import { createStore } from "vuex"

// 创建一个新的 store 实例@/node_modules/vuex/dist/vuex.esm.js
const store = createStore({
  state: {
      // 用户信息
      user: null,
      merchant:null,
      rider:null,
  },
  mutations: {  
    SET_USER(state, user) {  
        state.user = user;  
    },  
    CLEAR_USER(state) {  
        state.user = null;  
    },  
    SET_MERCHANT(state, merchant) {  
        state.merchant = merchant;  
    },  
    CLEAR_MERCHANT(state) {  
        state.merchant = null;  
    },  
},  
actions: {  
    setUser({ commit }, user) {  
        commit('SET_USER', user);  
    },  
    clearUser({ commit }) {  
        commit('CLEAR_USER');  
    },  
    setMerchant({ commit }, merchant) {  
        commit('SET_MERCHANT', merchant);  
    },  
    clearMerchant({ commit }) {  
        commit('CLEAR_MERCHANT');  
    },  
  },  
})


export default store