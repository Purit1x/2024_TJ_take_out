import { createStore } from "vuex"

// 创建一个新的 store 实例
const store = createStore({
  state() {
    return {
      // 用户信息
      user: {},
      merchant:{},
      rider:{},
    }
  }
})


export default store